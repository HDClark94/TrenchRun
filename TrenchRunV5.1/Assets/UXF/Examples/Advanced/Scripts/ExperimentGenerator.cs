using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

namespace UXFExamples
{

    public class ExperimentGenerator : MonoBehaviour
    {
        public void Generate(Session session)
		{
			int numTrials = System.Convert.ToInt32(session.settings["trials_per_block"]);

			// create two blocks
			Block block1 = session.CreateBlock(numTrials);
            Block block2 = session.CreateBlock(numTrials);
            
			// add catch trials
			MakeCatchTrials(block1);
            MakeCatchTrials(block2);

			// for each trial in the session, 50/50 chance of correct target being on left or right
            foreach (Trial trial in session.trials)
			{
                TargetPosition pos = Random.value > 0.5 ? TargetPosition.Left : TargetPosition.Right;
				trial.settings["correct_target_position"] = pos;
			}

			// set the block to be inverted ("go to the opposite target") or not, depending on the participant
            bool invertedBlockFirst;

			try
			{
                invertedBlockFirst = (bool) session.participantDetails["inverted_block_first"];
			}
			catch (System.NullReferenceException)
			{
                // during quick start mode, there are no participant details, so we get null reference exception
                invertedBlockFirst = Random.value > 0.5;
				Debug.LogFormat("Inverted block first: {0}", invertedBlockFirst);
			}			

			
			if (invertedBlockFirst)
			{
                block1.settings["inverted"] = true;
                block2.settings["inverted"] = false;
			}
			else
			{
				block1.settings["inverted"] = false;
                block2.settings["inverted"] = true;
			}

		}

		/// <summary>
		/// Modify a block by adding several catch trials and then shuffling the trial list.
		/// </summary>
		/// <param name="block"></param>
		void MakeCatchTrials(Block block)
		{
			int numCatchTrials = System.Convert.ToInt32(block.settings["catch_trials_per_block"]);
			
			if (numCatchTrials > block.trials.Count)
			{
				throw new System.Exception("There shouldn't be more catch trials than total trials per block!");
			}

			for (int i = 0; i < numCatchTrials; i++)
			{
				// double the existing delay time during catch trials
				Trial trial = block.trials[i];
                trial.settings["delay_time"] = 2 * System.Convert.ToSingle(trial.settings["delay_time"]);
			}

			// shuffle the trial order, so the catch trials are in random positions
			block.trials.Shuffle();
		}

    }
}