using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DuckbearLab.UniSim.Networking
{
    public class DISExerciseConnection : IDisposable
    {
        public DISEntitiesManager DISEntitiesManager { get; private set; }
        public byte ExerciseID;

        private UdpClient _udpClient;
        private IPEndPoint _broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 3000);

        private Dictionary<PDUType, PDUTypeContainer> _pdusContainers;

        private uint _currentTimeStamp;

        public DISExerciseConnection()
        {
            _udpClient = new UdpClient();
            _pdusContainers = new Dictionary<PDUType, PDUTypeContainer>();

            DISEntitiesManager = new DISEntitiesManager(this);
        }

        public void Dispose()
        {
            DISEntitiesManager.Dispose();
            _udpClient.Dispose();
            _udpClient = null;
        }

        public void Start()
        {
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 3000));

            _udpClient.BeginReceive(Receive, null);

            UpdateTimeStamp();
        }

        private void Receive(IAsyncResult ar)
        {
            if (_udpClient == null)
                return;

            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            var bytes = _udpClient.EndReceive(ar, ref ip);
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                PDUHeader pdu = new PDUHeader();
                PDUEncoder.Decode(pdu, reader);

                if (pdu.ExerciseId == ExerciseID)
                {
                    var pduType = pdu.PDUType;
                    PDUTypeContainer pduContainer;
                    if(_pdusContainers.TryGetValue(pduType, out pduContainer))
                        pduContainer.EnqueueIncomingPDU(reader);
                }
            }

            _udpClient.BeginReceive(Receive, null);
        }

        public void Tick()
        {
            foreach (var pduContainer in _pdusContainers.Values)
                pduContainer.NotifySubscribers();

            UpdateTimeStamp();
            DISEntitiesManager.Tick();
        }

        public void Subscribe<PDUType>(Action<PDUType> callback) where PDUType : PDU, new()
        {
            var pduType = new PDUType().PDUType;

            PDUTypeContainer pduContainer;
            if(!_pdusContainers.TryGetValue(pduType, out pduContainer))
            {
                pduContainer = new PDUTypeContainer(typeof(PDUType));
                _pdusContainers[pduType] = pduContainer;
            }

            pduContainer.Subscribe(callback);
        }

        public void Unsubscribe<PDUType>(Action<PDUType> callback) where PDUType : PDU, new()
        {
            var pduType = (new PDUType().PDUType);

            PDUTypeContainer pduContainer;
            if (_pdusContainers.TryGetValue(pduType, out pduContainer))
                pduContainer.Unsubscribe(callback);
        }

        private class PDUTypeContainer
        {
            private Type _type;
            private HashSet<Action<PDU>> _subscripitions;
            private Queue<PDU> _incomingPDUsQueue;
            private Dictionary<object, Action<PDU>> _rawCallbacks;

            public PDUTypeContainer(Type type)
            {
                _type = type;
                _subscripitions = new HashSet<Action<PDU>>();
                _incomingPDUsQueue = new Queue<PDU>();
                _rawCallbacks = new Dictionary<object, Action<PDU>>();
            }

            public void EnqueueIncomingPDU(BinaryReader reader)
            {
                var pdu = (PDU)Activator.CreateInstance(_type);
                PDUEncoder.Decode(pdu, reader);
                _incomingPDUsQueue.Enqueue(pdu);
            }

            public void NotifySubscribers()
            {
                while (_incomingPDUsQueue.Count > 0)
                {
                    var pdu = _incomingPDUsQueue.Dequeue();
                    foreach (var subscription in _subscripitions)
                        subscription(pdu);
                }
            }

            public void Subscribe<PDUType>(Action<PDUType> callback) where PDUType : PDU, new()
            {
                Action<PDU> wrappedCallback = (PDU pdu) => callback((PDUType)pdu);
                _rawCallbacks[callback] = wrappedCallback;
                _subscripitions.Add(wrappedCallback);
            }

            public void Unsubscribe<PDUType>(Action<PDUType> callback) where PDUType : PDU, new()
            {
                Action<PDU> wrappedCallback = _rawCallbacks[callback];
                _rawCallbacks.Remove(callback);
                _subscripitions.Remove(wrappedCallback);
            }
        }

        public void SendPDU(PDU pdu)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    var header = new PDUHeader()
                    {
                        ProtocolVersion = 7,
                        ExerciseId = ExerciseID,
                        PDUType = pdu.PDUType,
                        ProtocolFamilyType = 1,
                        TimeStamp = _currentTimeStamp
                    };

                    header.PDULength = (ushort)(PDUEncoder.Size(header) + PDUEncoder.Size(pdu));

                    PDUEncoder.Encode(header, writer);
                    PDUEncoder.Encode(pdu, writer);

                    var bytes = stream.ToArray();
                    _udpClient.Send(bytes, bytes.Length, _broadcastEndpoint);
                }
            }

        }

        private void UpdateTimeStamp()
        {
            double ticksFromStartOfHour = DateTime.Now.Ticks % TimeSpan.TicksPerHour;
            double fractionOfHour = ticksFromStartOfHour / TimeSpan.TicksPerHour;
            uint unitsInHour = ((uint)1 << 31) - 1; // According to DIS

            _currentTimeStamp = ((uint)(fractionOfHour * unitsInHour) << 1) + 0/* Relative time */;
        }

    }
}