using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim
{
    [DefaultExecutionOrder(-99)]
    [AddComponentMenu("UniSim/Entities Manager")]
    public class EntitiesManager : MonoBehaviour
    {
        public static EntitiesManager Instance;

        public event Action<ReflectedEntity> EntityJoined;
        public event Action<ReflectedEntity> EntityLeft;

        [SerializeField] private ExerciseConnection _exerciseConnection = null;
        [SerializeField] private EntitiesMapping _entitiesMapping = null;

        private DISEntitiesManager _disEntitiesManager;

        private Dictionary<DISReflectedEntity, ReflectedEntity> _reflectedEntities;

        private void Awake()
        {
            Instance = this;
            _reflectedEntities = new Dictionary<DISReflectedEntity, ReflectedEntity>();

            _disEntitiesManager = _exerciseConnection.DISExerciseConnection.DISEntitiesManager;
        }

        private void Start()
        {
            _disEntitiesManager.DISEntityJoined += DISEntityJoined;
            _disEntitiesManager.DISEntityLeft += DISEntityLeft;
        }

        private void OnDestroy()
        {
            _disEntitiesManager.DISEntityJoined -= DISEntityJoined;
            _disEntitiesManager.DISEntityLeft -= DISEntityLeft;
        }

        private void DISEntityJoined(DISReflectedEntity disReflectedEntity)
        {
            var entityType = disReflectedEntity.State.EntityType;
            var prefab = _entitiesMapping.GetReflectedEntityPrefab(entityType);
            var reflectedEntity = Instantiate(prefab);

            reflectedEntity.Initialize(disReflectedEntity);
            _reflectedEntities.Add(disReflectedEntity, reflectedEntity);
            EntityJoined?.Invoke(reflectedEntity);
        }

        private void DISEntityLeft(DISReflectedEntity disReflectedEntity)
        {
            ReflectedEntity reflectedEntity;
            if(_reflectedEntities.TryGetValue(disReflectedEntity, out reflectedEntity))
            {
                EntityLeft?.Invoke(reflectedEntity);
                Destroy(reflectedEntity.gameObject);
                _reflectedEntities.Remove(disReflectedEntity);
            }
        }

        private void Update()
        {
            foreach (var reflectedEntity in _reflectedEntities.Values)
                reflectedEntity.Tick();
        }

        public DISEntityPublisher CreateEntityPublisher()
        {
            return _disEntitiesManager.CreateEntityPublisher();
        }

        public void DisposeEntityPublisher(DISEntityPublisher disEntityPublisher)
        {
            _disEntitiesManager.DisposeEntityPublisher(disEntityPublisher);
        }

    }
}
