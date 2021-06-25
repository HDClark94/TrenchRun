﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UXF;

namespace UXFExamples
{

	public class InstructionsController : MonoBehaviour
	{
		
		[TextArea]
		public string normalInstructions;
        [TextArea]
		public string invertedInstructions;

		public Session session;

		Text text;

		void Awake()
		{
			text = GetComponent<Text>();
            text.text = string.Empty;
		}

		public void UpdateInstructions()
		{
			// we actually want to update the instructions for the next trial, not the trial that has just ended
			Trial nextTrial;

			try
			{
                nextTrial = session.nextTrial;
			}
			catch (NoSuchTrialException)
			{
				// reached end of session
                text.text = string.Empty;
				return;
			}

            bool inverted = (bool)nextTrial.settings["inverted"];
            text.text = inverted ? invertedInstructions : normalInstructions;

		}

	}

}