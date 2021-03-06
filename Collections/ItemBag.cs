﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Plugins.Common.Collections {
  public class ItemBag: IEnumerable<ItemData>, ICollection<ItemData> {
    protected List<ItemData> InternalList { get; private set; }
    public int MaxItemSlots { get; private set; }

    public int Count { 
      get { return this.InternalList.Count; }
    }

    public ItemData LatestItem {
      get {
        if (this.InternalList.Count == 0)
          return ItemData.None;

        return this.InternalList[this.InternalList.Count - 1];
      }
    }


    public ItemBag(int maxItemSlots = -1) {
      this.InternalList = new List<ItemData>(maxItemSlots == -1 ? 10 : maxItemSlots);
      this.MaxItemSlots = maxItemSlots;
    }

    public void Add(ItemData item) {
      Item dummyTItem = new Item();
      dummyTItem.netDefaults((int)item.Type);

      int itemMaxStack = dummyTItem.maxStack;
      for (int i = this.InternalList.Count - 1; i >= 0; i--) {
        ItemData bagItem = this.InternalList[i];
        if (bagItem.Prefix != item.Prefix || bagItem.Type != item.Type)
          continue;

        if (bagItem.StackSize < itemMaxStack) {
          int available = itemMaxStack - bagItem.StackSize;
          if (item.StackSize <= available) {
            bagItem.StackSize += item.StackSize;
            this.InternalList[i] = bagItem;
            return;
          } else {
            bagItem.StackSize = itemMaxStack;
            this.InternalList[i] = bagItem;

            item.StackSize -= available;
          }
        }
      }

      if (item.StackSize > 0) {
        if (this.InternalList.Count == this.MaxItemSlots)
          this.InternalList.RemoveAt(0);

        this.InternalList.Add(item);
      }
    }

    public void CopyTo(ItemData[] array, int arrayIndex) {
      this.InternalList.CopyTo(array, arrayIndex);
    }

    public void Clear() {
      this.InternalList.Clear();
    }

    public IEnumerator<ItemData> GetEnumerator() {
      return this.InternalList.GetEnumerator();
    }

    public bool Remove(ItemData item) {
      for (int i = this.InternalList.Count - 1; i >= 0; i--) {
        ItemData bagItem = this.InternalList[i];
        if (bagItem.Prefix != item.Prefix || bagItem.Type != item.Type)
          continue;

        if (bagItem.StackSize == item.StackSize) {
          this.InternalList.RemoveAt(i);
          return true;
        } else if (bagItem.StackSize > item.StackSize) {
          bagItem.StackSize -= item.StackSize;
          this.InternalList[i] = bagItem;
          return true;
        } else {
          item.StackSize -= bagItem.StackSize;
          this.InternalList.RemoveAt(i);
        }
      }

      return true;
    }

    public bool RemoveExact(ItemData item) {
      return this.InternalList.Remove(item);
    }

    public bool RemoveLastExact(ItemData item) {
      for (int i = this.InternalList.Count - 1; i >= 0; i--) {
        if (this.InternalList[i] == item) {
          this.InternalList.RemoveAt(i);
          return true;
        }
      }

      return false;
    }

    public bool Contains(ItemData item) {
      int stackCounter = 0;
      for (int i = this.InternalList.Count - 1; i >= 0; i--) {
        ItemData bagItem = this.InternalList[i];
        if (bagItem.Prefix != item.Prefix || bagItem.Type != item.Type)
          continue;

        stackCounter += bagItem.StackSize;
        if (stackCounter >= item.StackSize)
          return true;
      }

      return false;
    }

    public bool ContainsExact(ItemData item) {
      foreach (ItemData bagItem in this)
        if (bagItem == item)
          return true;

      return false;
    }

    public bool ContainsCoinValue(int coinValue) {
      for (int i = this.InternalList.Count - 1; i >= 0; i--) {
        ItemData bagItem = this.InternalList[i];
        if (!TerrariaUtils.Items.IsCoinType(bagItem.Type))
          continue;

        coinValue -= TerrariaUtils.Items.GetCoinItemValue(bagItem);
        if (coinValue <= 0)
          return true;
      }

      return false;
    }

    public int GetTotalCoinValue() {
      int totalCoinValue = 0;
      foreach (ItemData bagItem in this)
        if (TerrariaUtils.Items.IsCoinType(bagItem.Type))
          totalCoinValue += TerrariaUtils.Items.GetCoinItemValue(bagItem);

      return totalCoinValue;
    }

    #region [ICollection Implementation]
    bool ICollection<ItemData>.IsReadOnly {
      get { return false; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }
    #endregion
  }
}
