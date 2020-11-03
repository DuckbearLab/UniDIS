using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim
{
    [CreateAssetMenu]
    public class EntitiesMapping : ScriptableObject
    {

        [System.Serializable]
        public class EntityMapping
        {
            public string Name;
            public string EntityType;
            public ReflectedEntity Prefab;
        }

        [SerializeField] private EntityMapping[] _mappings = null;
        [SerializeField] private ReflectedEntity _defaultPrefab = null;

        private Dictionary<EntityType, EntityMapping> _mappingDict = null;

        private void OnEnable()
        {
            if (_mappings == null)
                _mappings = new EntityMapping[0];

            _mappingDict = new Dictionary<EntityType, EntityMapping>();

            foreach (var mapping in _mappings)
                _mappingDict[EntityType.FromString(mapping.EntityType)] = mapping;
        }

        public ReflectedEntity GetReflectedEntityPrefab(EntityType entityType)
        {
            EntityMapping foundMapping;
            if (_mappingDict.TryGetValue(entityType, out foundMapping))
                return foundMapping.Prefab;
            return _defaultPrefab;
        }
    }
}