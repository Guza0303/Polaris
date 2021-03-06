﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Observe
{
	public class ObsSkyAreaBtn : MonoBehaviour
	{
		public int index;
		public int price;
		[TextArea] public string openRule;
		public Text priceLabel;
		public Text openRuleLabel;
		public GameObject pricePanel;
		public GameObject openRulePanel;
		public ObserveManager manager;

		void Start()
		{
			priceLabel.text = price.ToString();
			openRuleLabel.text = openRule;

			pricePanel.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.05f;
		}

		public void Clicked()
		{
			if (Variables.Starlight < price)
				manager.NoMoneyPanel.SetActive(true);
			else
				manager.UnlockAskPanels[index].SetActive(true);
		}
	}
}