using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prax.Recognition
{
    static class Extensions
    {
        public static int Area(this Rectangle rect) { return rect.Width * rect.Height; }

        
                 #region FindAggregate
                 ///<summary>Finds the best item in a sequence.</summary>
                 ///<typeparam name="TSource">The type of the items in the sequence.</typeparam>
                  ///<typeparam name="TProperty">The type of the property used to compare the items.</typeparam>
                 ///<param name="items">The sequence to search.</param>
                  ///<param name="propertySelector">An expression that returns the property to compare items by.</param>
                  ///<param name="comparer">An expression that determines which of two properties is better.</param>
                 ///<returns>The best item, or default(TSource) if the sequence is empty.</returns>
                  public static TSource FindAggregate<TSource, TProperty>(this IEnumerable<TSource> items, Func<TSource, TProperty> propertySelector, Func<TProperty, TProperty, TProperty> comparer) {
                          if (items == null) throw new ArgumentNullException("items");
                          if (propertySelector == null) throw new ArgumentNullException("propertySelector");
                          if (comparer == null) throw new ArgumentNullException("comparer");

                          TSource bestItem = default(TSource);
                          TProperty bestStat = default(TProperty);

                          bool first = true;
                          foreach (var item in items) {
                                   if (first) {
                                            bestItem = item;
                                            bestStat = propertySelector(item);
                                            first = false;
                                            continue;
                                   }

                                   var stat = propertySelector(item);
                                   var betterStat = comparer(stat, bestStat);
                                   if (Equals(stat, betterStat)) {    //If our item is better
                                            bestItem = item;
                                            bestStat = stat;
                                   }
                          }
                          return bestItem;
                 }
                 ///<summary>Finds the item with the highest property in a sequence.</summary>
                  public static T FindMax<T>(this IEnumerable<T> items, Func<T, Byte> selector) { return items.FindAggregate(selector, Math.Max); }
                 ///<summary>Finds the item with the highest property in a sequence.</summary>
                  public static T FindMax<T>(this IEnumerable<T> items, Func<T, Int16> selector) { return items.FindAggregate(selector, Math.Max); }
                 ///<summary>Finds the item with the highest property in a sequence.</summary>
                  public static T FindMax<T>(this IEnumerable<T> items, Func<T, Int32> selector) { return items.FindAggregate(selector, Math.Max); }
                 ///<summary>Finds the item with the highest property in a sequence.</summary>
                  public static T FindMax<T>(this IEnumerable<T> items, Func<T, Int64> selector) { return items.FindAggregate(selector, Math.Max); }
                 ///<summary>Finds the item with the highest property in a sequence.</summary>
                  public static T FindMax<T>(this IEnumerable<T> items, Func<T, Single> selector) { return items.FindAggregate(selector, Math.Max); }
                 ///<summary>Finds the item with the highest property in a sequence.</summary>
                  public static T FindMax<T>(this IEnumerable<T> items, Func<T, Double> selector) { return items.FindAggregate(selector, Math.Max); }
                 ///<summary>Finds the item with the highest property in a sequence.</summary>
                  public static T FindMax<T>(this IEnumerable<T> items, Func<T, Decimal> selector) { return items.FindAggregate(selector, Math.Max); }

                 ///<summary>Finds the item with the lowest property in a sequence.</summary>
                  public static T FindMin<T>(this IEnumerable<T> items, Func<T, Byte> selector) { return items.FindAggregate(selector, Math.Min); }
                 ///<summary>Finds the item with the lowest property in a sequence.</summary>
                  public static T FindMin<T>(this IEnumerable<T> items, Func<T, Int16> selector) { return items.FindAggregate(selector, Math.Min); }
                 ///<summary>Finds the item with the lowest property in a sequence.</summary>
                  public static T FindMin<T>(this IEnumerable<T> items, Func<T, Int32> selector) { return items.FindAggregate(selector, Math.Min); }
                 ///<summary>Finds the item with the lowest property in a sequence.</summary>
                  public static T FindMin<T>(this IEnumerable<T> items, Func<T, Int64> selector) { return items.FindAggregate(selector, Math.Min); }
                 ///<summary>Finds the item with the lowest property in a sequence.</summary>
                  public static T FindMin<T>(this IEnumerable<T> items, Func<T, Single> selector) { return items.FindAggregate(selector, Math.Min); }
                 ///<summary>Finds the item with the lowest property in a sequence.</summary>
                  public static T FindMin<T>(this IEnumerable<T> items, Func<T, Double> selector) { return items.FindAggregate(selector, Math.Min); }
                 ///<summary>Finds the item with the lowest property in a sequence.</summary>
                  public static T FindMin<T>(this IEnumerable<T> items, Func<T, Decimal> selector) { return items.FindAggregate(selector, Math.Min); }
                 #endregion

                 ///<summary>Adds zero or more items to a collection.</summary>
                  public static void AddRange<TItem, TElement>(this ICollection<TElement> collection, params TItem[] items) where TItem : TElement { collection.AddRange((IEnumerable<TItem>)items); }
                 ///<summary>Adds zero or more items to a collection.</summary>
                  public static void AddRange<TItem, TElement>(this ICollection<TElement> collection, IEnumerable<TItem> items)
                          where TItem : TElement {
                          if (collection == null) throw new ArgumentNullException("collection");
                          if (items == null) throw new ArgumentNullException("items");

                          foreach (var item in items)
                                   collection.Add(item);
                 }

        ///<summary>Measures the exact size of a string.</summary>
        ///<param name="graphics">A Graphics object to measure the string on.</param>
        ///<param name="text">The string to measure.</param>
        ///<param name="font">The font used to draw the string.</param>
        ///<returns>The exact width of the string in pixels.</returns>
        public static SizeF MeasureStringSize(this Graphics graphics, string text, Font font)
        {
            if (graphics == null) throw new ArgumentNullException("graphics");
            if (text == null) throw new ArgumentNullException("text");
            if (font == null) throw new ArgumentNullException("font");

            using (var format = new StringFormat())
            {
                format.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, text.Length) });

                using (var region = graphics.MeasureCharacterRanges(text, font, new Rectangle(0, 0, int.MaxValue / 2, int.MaxValue / 2), format)[0])
                {
                    return region.GetBounds(graphics).Size;
                }
            }
        }
    }
}
