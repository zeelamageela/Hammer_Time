using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;

[System.Serializable]
public class Inventory_List : IComparable<Inventory_List>
{
    public string name;
    public Equipment equipment;

    public Inventory_List(Equipment newEquipment)
    {
        equipment = newEquipment;
        name = newEquipment.name;
    }

    public int CompareTo(Inventory_List other)
    {
        if (other == null)
        {
            return 1;
        }
        else if (equipment.id > other.equipment.id)
        {
            return 1;
        }
        else if (equipment.id < other.equipment.id)
        {
            return -1;
        }
        else if (equipment.id == other.equipment.id)
        {
            return 0;
        }
        else return 0;
    }
}
