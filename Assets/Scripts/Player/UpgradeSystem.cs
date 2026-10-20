using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    public class UpgradeOption
    {
        public string Title;
        public string Description;
        public string Icon;
        public Action Apply;
    }

    /// <summary>Sorteia as opções do level up: armas novas, níveis de arma e melhorias passivas.</summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public PlayerController player;

        public List<UpgradeOption> Roll(int count)
        {
            var pool = new List<UpgradeOption>();
            foreach (var w in player.GetComponents<Weapon>())
            {
                if (w.Level >= Weapon.MaxLevel) continue;
                var weapon = w;
                var opt = new UpgradeOption
                {
                    Title = w.Level == 0 ? $"NOVA: {w.DisplayName}" : $"{w.DisplayName} Nv.{w.Level + 1}",
                    Description = w.DescribeNext(),
                    Icon = w.IconName,
                    Apply = weapon.LevelUp,
                };
                pool.Add(opt);
                pool.Add(opt); // armas têm o dobro de chance
            }
            pool.Add(new UpgradeOption
            {
                Title = "Placa de Blindagem", Description = "+20 de vida máxima e cura 20.", Icon = "icon_heart",
                Apply = () => player.AddMaxHealth(20f),
            });
            pool.Add(new UpgradeOption
            {
                Title = "Servos nas Pernas", Description = "+10% de velocidade.", Icon = "icon_speed",
                Apply = () => player.moveSpeed *= 1.1f,
            });
            pool.Add(new UpgradeOption
            {
                Title = "Ímã de Dados", Description = "+40% de alcance para coletar chips.", Icon = "icon_magnet",
                Apply = () => player.magnetRadius *= 1.4f,
            });
            pool.Add(new UpgradeOption
            {
                Title = "Overclock", Description = "+15% de dano em todas as armas.", Icon = "icon_damage",
                Apply = () => player.damageMultiplier += 0.15f,
            });

            var picked = new List<UpgradeOption>();
            while (picked.Count < count && pool.Count > 0)
            {
                var o = pool[UnityEngine.Random.Range(0, pool.Count)];
                pool.RemoveAll(x => x == o);
                picked.Add(o);
            }
            return picked;
        }
    }
}
