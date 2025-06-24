using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

public class EnemyManager : MonoBehaviour
{
   [SerializeField] public List<GameObject> Enemies;
   
   [SerializeField] private List<Animator> _enemyAnimators = new List<Animator>();

   [SerializeField] private List<NavMeshAgent> _navMeshAgents = new List<NavMeshAgent>();
   
   private void Awake()
   {
      foreach (var _enemy in Enemies)
      {
        _enemyAnimators.Add(_enemy.GetComponent<Animator>()); 
      }
      foreach (var _enemy in Enemies)
      {
         _navMeshAgents.Add(_enemy.GetComponent<NavMeshAgent>()); 
      }
   }
   
   public void SlowEnemies(float slowTime)
   {
      Debug.Log("Slow Enemies");

      for (int i = 0; i < Enemies.Count; i++)
      {
         if(_enemyAnimators[i] == null||_navMeshAgents[i] == null)continue;
         
         // _navMeshAgents[i].isStopped = true;

         var _anim = _enemyAnimators[i];
         
         var sequence = DOTween.Sequence();

         sequence.Append(DOTween.To(() => _anim.speed, x => _anim.speed = x, 0.1f, slowTime * 0.3f)
               .SetEase(Ease.OutCubic))
            .Append(DOTween.To(() => _anim.speed, x => _anim.speed = x, 1, slowTime * 0.7f).SetEase(Ease.InCubic))
            .OnComplete(() =>
            {
               // _navMeshAgents[i].isStopped = false;
            });
      }
   }
}
