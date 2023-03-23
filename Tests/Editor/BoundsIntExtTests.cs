using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

//https://docs.unity3d.com/Manual/cus-tests.html
namespace Kutil.Tests {
    public class BoundsIntExtTests {
        // A Test behaves as an ordinary method
        [Test]
        public void BoundsIntExtTestsSimplePasses() {
            // Use the Assert class to test conditions
        }
    
        [Test]
        public void SplitBoundsTest() {
            BoundsInt b = new();
            b.SetMinMax(new(-10, -10, -10), new(10, 10, 10));
            Vector3Int splitPoint = Vector3Int.zero;
            BoundsInt[] cutBounds = BoundsIntExtensions.SplitBounds(b, splitPoint).ToArray();
            Assert.AreEqual(8, cutBounds.Length);
            BoundsInt[] expected = new BoundsInt[] {
                    new(-10,-10,-10, 10,10,10),
                    new(-10,-10,  0, 10,10,10),
                    new(-10,  0,-10, 10,10,10),
                    new(-10,  0,  0, 10,10,10),
                    new(  0,-10,-10, 10,10,10),
                    new(  0,-10,  0, 10,10,10),
                    new(  0,  0,-10, 10,10,10),
                    new(  0,  0,  0, 10,10,10),
                };
            // Debug.Log(cutBounds.ToStringFull(null, true));
            System.Array.Sort(expected, new BoundsIntExtensions.BoundsIntComparer());
            System.Array.Sort(cutBounds, new BoundsIntExtensions.BoundsIntComparer());
            Assert.AreEqual(expected, cutBounds);
        }
        [Test]
        public void SplitBoundsTest2() {
            BoundsInt b = new();
            b.SetMinMax(new(-10, 4, 4), new(10, 5, 5));
            Vector3Int splitPoint = Vector3Int.zero;
            BoundsInt[] cutBounds = BoundsIntExtensions.SplitBounds(b, splitPoint).ToArray();
            BoundsInt[] expected = new BoundsInt[] {
                    new(-10,4,4,10,1,1),
                    new(  0,4,4,10,1,1),
                };
            System.Array.Sort(expected, new BoundsIntExtensions.BoundsIntComparer());
            System.Array.Sort(cutBounds, new BoundsIntExtensions.BoundsIntComparer());
            Assert.AreEqual(expected, cutBounds);
        }
    }
}