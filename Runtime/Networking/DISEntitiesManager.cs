using System;
using System.Collections;
using System.Collections.Generic;

namespace DuckbearLab.UniSim.Networking
{
    public class DISEntitiesManager : IDisposable
    {
        public event Action<DISReflectedEntity> DISEntityJoined;
        public event Action<DISReflectedEntity> EntityUpdated;
        public event Action<DISReflectedEntity> DISEntityLeft;

        private const float HeartbeatTimeoutPeriod = 15f;

        private DISExerciseConnection _disExerciseConnection;

        private List<DISEntityPublisher> _entityPublishers;
        private HashSet<EntityId> _publishedEntitiesIds;

        private Dictionary<EntityId, DISReflectedEntity> _reflectedEntities;

        public DISEntitiesManager(DISExerciseConnection disExerciseConnection)
        {
            _disExerciseConnection = disExerciseConnection;

            _entityPublishers = new List<DISEntityPublisher>();
            _publishedEntitiesIds = new HashSet<EntityId>();
            _reflectedEntities = new Dictionary<EntityId, DISReflectedEntity>();

            _disExerciseConnection.Subscribe<EntityStatePDU>(ReceivedEntityStatePDU);
        }

        public void Dispose()
        {
            _disExerciseConnection.Unsubscribe<EntityStatePDU>(ReceivedEntityStatePDU);
        }

        private void ReceivedEntityStatePDU(EntityStatePDU entityState)
        {
            if (IsOwnEntity(entityState))
                return;

            DISReflectedEntity reflectedEntity;

            if (!_reflectedEntities.TryGetValue(entityState.EntityId, out reflectedEntity))
            {
                reflectedEntity = new DISReflectedEntity();
                _reflectedEntities[entityState.EntityId] = reflectedEntity;

                reflectedEntity.UpdateState(entityState);
                DISEntityJoined?.Invoke(reflectedEntity);
            }
            else
            {
                reflectedEntity.UpdateState(entityState);
                EntityUpdated?.Invoke(reflectedEntity);
            }
        }

        public void Tick()
        {
            _publishedEntitiesIds.Clear();
            foreach (var entityPublisher in _entityPublishers)
            {
                _publishedEntitiesIds.Add(entityPublisher.State.EntityId);
                entityPublisher.Tick();
            }

            List<DISReflectedEntity> timedOutEntities = new List<DISReflectedEntity>();

            foreach (var reflectedEntity in _reflectedEntities.Values)
            {
                if (reflectedEntity.TimeSinceLastHeartbeat >= HeartbeatTimeoutPeriod)
                    timedOutEntities.Add(reflectedEntity);
            }

            foreach (var timedOutEntity in timedOutEntities)
            {
                DISEntityLeft?.Invoke(timedOutEntity);
                _reflectedEntities.Remove(timedOutEntity.State.EntityId);
            }
        }

        public DISEntityPublisher CreateEntityPublisher()
        {
            var disEntityPublisher = new DISEntityPublisher(_disExerciseConnection);
            _entityPublishers.Add(disEntityPublisher);
            return disEntityPublisher;
        }

        public void DisposeEntityPublisher(DISEntityPublisher disEntityPublisher)
        {
            _entityPublishers.Remove(disEntityPublisher);
        }

        private bool IsOwnEntity(EntityStatePDU entityState)
        {
            return _publishedEntitiesIds.Contains(entityState.EntityId);
        }
    }
}
