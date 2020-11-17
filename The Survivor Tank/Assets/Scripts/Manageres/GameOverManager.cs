using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public PlayerHealth playerHealth;       // Reference to the player's health.

    Animator anim;                          // Reference to the animator component.

    void Awake()
    {
	    // Set up the reference.
        anim = GetComponent<Animator>();
        
        var metaData = new EventMetaData {amount = 1, event_type = "start"};
        SCILLManager.Instance.SendEventAsync("trigger-event", "single", metaData);
    }

    private void Start()
    {
		StartCoroutine(CheckGameOver());
    }

    IEnumerator CheckGameOver()
	{
        // While the player has health...
        while (playerHealth.currentHealth > 0)
		{
			yield return new WaitForSeconds(0.3f);
		}

        // Tell the animator the game is over...
        anim.SetTrigger("GameOver");
        
        var metaData = new EventMetaData {amount = 1, event_type = "end"};
        SCILLManager.Instance.SendEventAsync("trigger-event", "single", metaData);

        if (ScoreManager.score > 0)
        {
	        metaData = new EventMetaData {score = ScoreManager.score};
	        SCILLManager.Instance.SendEventAsync("achieve-score", "group", metaData);			
        }

        yield return new WaitForSeconds(3f);
		
		// Reload the level that is currently loaded.
		SceneManager.LoadScene(0);
	}
}
