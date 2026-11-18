using UnityEngine;
using Emberfall.Core;
using Emberfall.Player;

namespace Emberfall.World
{
    public class Goal : MonoBehaviour
    {
        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (other.GetComponent<PlayerHealth>() == null) return;

            _triggered = true;
            other.GetComponent<PlayerController2D>()?.EnterLevelComplete();
            GameManager.Instance?.CompleteCurrentLevel();
            GameEvents.RaiseLevelCompleted();
        }
    }
}
