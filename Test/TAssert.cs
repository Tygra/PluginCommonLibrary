﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.Common.Test {
  public static class TAssert {
    public static void IsObjectActive(int x, int y) {
      TAssert.IsObjectActive(x, y, true);
    }

    public static void IsObjectInactive(int x, int y) {
      TAssert.IsObjectActive(x, y, false);
    }

    public static void IsObjectActive(int x, int y, bool expectedState) {
      Tile tile = TerrariaUtils.Tiles[x, y];
      if (!tile.active()) {
        throw new AssertException(
          string.Format("Assert failed. There is no tile at [{0},{1}].", x, y)
        );
      }

      ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(new DPoint(x, y));
      bool isActive = TerrariaUtils.Tiles.ObjectHasActiveState(measureData);
      if (isActive != expectedState) {
        string actualStateString;
        if (isActive)
          actualStateString = "active";
        else 
          actualStateString = "inactive";

        string expectedStateString;
        if (expectedState)
          expectedStateString = "Active";
        else
          expectedStateString = "Inactive";

        throw new AssertException(string.Format(
          "Assert failed. {0} frame for object \"{1}\" at [{2},{3}] was expected, but it is {4}.", 
          expectedStateString, TerrariaUtils.Tiles.GetBlockTypeName((BlockType)tile.type), x, y, actualStateString
        ));
      }
    }

    public static void IsTileActive(int x, int y) {
      TAssert.IsTileActive(x, y, true);
    }

    public static void IsTileInactive(int x, int y) {
      TAssert.IsTileActive(x, y, false);
    }

    public static void IsTileActive(int x, int y, bool expectedState) {
      Tile tile = TerrariaUtils.Tiles[x, y];
      if (tile.active() != expectedState) {
        string actualStateString;
        if (tile.active())
          actualStateString = "active";
        else 
          actualStateString = "inactive";

        string expectedStateString;
        if (expectedState)
          expectedStateString = "Active";
        else
          expectedStateString = "Inactive";

        throw new AssertException(string.Format(
          "Assert failed. {0} state for tile at [{1},{2}] was expected, but it is {3}.", 
          expectedStateString, x, y, actualStateString
        ));
      }
    }

    public static void IsBlockType(int x, int y, BlockType expectedBlockType) {
      Tile tile = TerrariaUtils.Tiles[x, y];
      
      if (!tile.active()) {
        throw new AssertException(string.Format(
          "The tile id \"{0}\" was expected at [{1},{2}], but there is no tile at all.",
          TerrariaUtils.Tiles.GetBlockTypeName(expectedBlockType), x, y
        ));
      }

      if (tile.type != (int)expectedBlockType) {
        throw new AssertException(string.Format(
          "The tile id \"{0}\" was expected at [{1},{2}], but it is \"{3}\".",
          TerrariaUtils.Tiles.GetBlockTypeName(expectedBlockType), x, y, TerrariaUtils.Tiles.GetBlockTypeName((BlockType)tile.type)
        ));
      }
    }

    public static void HasLiquid(int x, int y) {
      Tile tile = TerrariaUtils.Tiles[x, y];
      
      if (tile.liquid <= 0) {
        throw new AssertException(string.Format(
          "The tile at [{0},{1}] was expected to have liquid, but it doesn't.", x, y
        ));
      }
    }

    public static void HasNoLiquid(int x, int y) {
      Tile tile = TerrariaUtils.Tiles[x, y];
      
      if (tile.liquid != 0) {
        throw new AssertException(string.Format(
          "The tile at [{0},{1}] was expected to have no liquid, but it has.", x, y
        ));
      }
    }

    public static void HasFullLiquid(int x, int y) {
      Tile tile = TerrariaUtils.Tiles[x, y];
      
      if (tile.liquid < 255) {
        throw new AssertException(string.Format(
          "The tile at [{0},{1}] was expected to have 255 liquid, but it has {2}.", x, y, tile.liquid
        ));
      }
    }

    public static void HasNotFullLiquid(int x, int y) {
      Tile tile = TerrariaUtils.Tiles[x, y];
      
      if (tile.liquid <= 0) {
        throw new AssertException(string.Format(
          "The tile at [{0},{1}] was expected to have at least 1 liquid, but it has none.", x, y
        ));
      }

      if (tile.liquid == 255) {
        throw new AssertException(string.Format(
          "The tile at [{0},{1}] was expected to have less than 255 liquid, but it has {2}.", x, y, tile.liquid
        ));
      }
    }

    public static void AreItemsInBlockRect(
      int tileX, int tileY, int tileW, int tileH, ItemType expectedItemType, int expectedCount, bool allowOtherNpcs = false
    ) {
      int x = tileX * TerrariaUtils.TileSize;
      int y = tileY * TerrariaUtils.TileSize;
      int r = x + (tileW * TerrariaUtils.TileSize);
      int b = y + (tileH * TerrariaUtils.TileSize);
      int count = 0;

      bool ofExpectedId = false;
      bool ofOtherIds = false;
      for (int i = 0; i < 200; i++) {
        Item item = Main.item[i];

        if (
          item.active && 
          item.position.X >= x && item.position.X <= r &&
          item.position.Y >= y && item.position.Y <= b
        ) {
          if (item.type == (int)expectedItemType) {
            if (!ofExpectedId)
              ofExpectedId = true;

            count++;
          } else {
            if (!ofOtherIds)
              ofOtherIds = true;
          }
        }
      }

      if (ofExpectedId) {
        if (count != expectedCount) {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] contains {4} of the items, {5} were expected though.",
            tileX, tileY, tileW, tileH, expectedCount, count
          ));
        }

        if (ofOtherIds && !allowOtherNpcs) {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] contains {4} of the items, though it contains other items aswell.",
            tileX, tileY, tileW, tileH, expectedCount
          ));
        }
      } else {
        if (ofOtherIds) {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] does not contain the expected item. There are items of other types though.",
            tileX, tileY, tileW, tileH
          ));
        } else {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] does not contain the expected item.",
            tileX, tileY, tileW, tileH
          ));
        }
      }
    }

    public static void AreNPCsInBlockRect(
      int tileX, int tileY, int tileW, int tileH, int expectedNPCType, int expectedCount, bool allowOtherItems = false
    ) {
      int x = tileX * TerrariaUtils.TileSize;
      int y = tileY * TerrariaUtils.TileSize;
      int r = x + (tileW * TerrariaUtils.TileSize);
      int b = y + (tileH * TerrariaUtils.TileSize);
      int count = 0;

      bool ofExpectedId = false;
      bool ofOtherIds = false;
      for (int i = 0; i < 200; i++) {
        NPC npc = Main.npc[i];

        if (
          npc.active && 
          npc.position.X >= x && npc.position.X <= r &&
          npc.position.Y >= y && npc.position.Y <= b
        ) {
          if (npc.type == expectedNPCType) {
            if (!ofExpectedId)
              ofExpectedId = true;

            count++;
          } else {
            if (!ofOtherIds)
              ofOtherIds = true;
          }
        }
      }

      if (ofExpectedId) {
        if (count != expectedCount) {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] contains {4} of the npcs, {5} were expected though.",
            tileX, tileY, tileW, tileH, expectedCount, count
          ));
        }

        if (ofOtherIds && !allowOtherItems) {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] contains {4} of the npcs, though it contains other npcs aswell.",
            tileX, tileY, tileW, tileH, expectedCount
          ));
        }
      } else {
        if (ofOtherIds) {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] does not contain the expected npcs. There are npcs of other types though.",
            tileX, tileY, tileW, tileH
          ));
        } else {
          throw new AssertException(string.Format(
            "The block rectangle [{0},{1},{2},{3}] does not contain the expected npcs.",
            tileX, tileY, tileW, tileH
          ));
        }
      }
    }

    public static bool ExpectException(Action action, Type expectedException) {
      try {
        action();
      } catch (Exception ex) {
        if (ex.GetType() == expectedException)
          return true;

        throw new AssertException(string.Format(
          "Asser failed. Expected exception \"{0}\" was not thrown, \"{1}\" was thrown instead.", 
          expectedException.Name, ex.GetType().Name
        ), ex);
      }

      throw new AssertException(string.Format("Asser failed. Expected exception \"{0}\" was not thrown.", expectedException.Name));
    }

    public static void Fail(string messageFormat, params object[] args) {
      throw new AssertException(string.Format(messageFormat, args));
    }
  }
}
