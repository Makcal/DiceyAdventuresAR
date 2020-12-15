//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Inventory : MonoBehaviour
//{

//	private Canvas canvas;

//	public GameObject player;

//	private Items items;

//	public Transform inventorySlots;

//	private Slot[] slots;

//	void Start()
//	{
//		canvas = GetComponent<Canvas>();
//		canvas.enabled = false;
//		items = player.GetComponent<Items>();
//		slots = inventorySlots.GetComponentsInChildren<Slot>(); 
//	}


//	void UpdateUI()
//	{
//		for (int i = 0; i < slots.Length; i++) //здесь как раз проверка предметов, Максим, мой дорогой друг
//		{
//			bool active = false;
//			if (items.hasItems[i])
//			{
//				active = true;
//			}

//			slots[i].UpdateSlot(active);
//		}
//	}
//}