using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinMaxLibrary.algorithms;
namespace AI
{
    public class TreeBasedUnitConf : PlayerConf
    {
        double x;
        bool isLostWhenScoreIsZero, hasWonWhenScoreIsOne;

        public TreeBasedUnitConf(double x, bool isLostWhenScoreIsZero = true, bool hasWonWhenScoreIsOne = false)
        {
            this.x = x;
            this.isLostWhenScoreIsZero = isLostWhenScoreIsZero;
            this.hasWonWhenScoreIsOne = hasWonWhenScoreIsOne;
        }

        public TreeBasedUnitConf()
        {
            double x;
            bool isLostWhenScoreIsZero, hasWonWhenScoreIsOne;
            x = 0.0;
        }

        public override double getScore()
        {
            return x;
        }

        public override bool hasPlayerLost()
        {
            return isLostWhenScoreIsZero ? (System.Math.Abs(x) < 0.001) : false;
        }

        public override bool hasPlayerWon()
        {
            return hasWonWhenScoreIsOne ? (System.Math.Abs(x - 1.0) < 0.001) : false;
        }

        public override PlayerConf clone()
        {
            return new TreeBasedUnitConf(x, isLostWhenScoreIsZero, hasWonWhenScoreIsOne);
        }
    }
}