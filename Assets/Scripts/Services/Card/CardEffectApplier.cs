using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OrderRush.Services
{
    public class CardEffectApplier
    {
        private readonly ILevelContextPresenter _levelPresenter;
        private readonly SpawnFactory _spawnFactory;

        public CardEffectApplier(
            ILevelContextPresenter levelPresenter,
            SpawnFactory spawnFactory)
        {
            _levelPresenter = levelPresenter;
            _spawnFactory = spawnFactory;
        }

        public async UniTask ApplyEffect(CardEffectData effect)
        {
            switch (effect.EffectType)
            {
                case EffectType.Table:
                    await ApplyTableAddition((TableAdditionEffect)effect);
                    break;
            }
        }

        private async UniTask ApplyTableAddition(TableAdditionEffect effect)
        {
            Vector3 position = CalculateNewTablePosition();

            var table = await _spawnFactory.CreatePersistent<DiningTable>(
                effect.TablePrefabName,
                position,
                _levelPresenter.LevelTransform);

            if (table != null)
            {
                _levelPresenter.AddDiningTable(table);
            }
        }

        private Vector3 CalculateNewTablePosition()
        {
            var tables = _levelPresenter.DiningTables;
            if (tables.Count == 0)
                return Vector3.zero;

            var lastTable = tables[tables.Count - 1];
            return lastTable.transform.position + new Vector3(2f, 0, 0);
        }
    }
}
