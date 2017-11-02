using Aimtec.SDK.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimtec.SDK.Prediction.Skillshots
{
    class PredictionImplB : ISkillshotPrediction
    {
        public static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();
        public static Vector3 PredictedPos;


        public PredictionOutput GetPrediction(PredictionInput input)
        {
            var cp = PredEx(input.Unit, input.Delay);
            return new PredictionOutput { CastPosition = cp, UnitPosition = cp, HitChance = HitChance.VeryHigh, };
        }

        public PredictionOutput GetPrediction(PredictionInput input, bool ft, bool collision)
        {
            var cp = PredEx(input.Unit, input.Delay);
            return new PredictionOutput { CastPosition = cp, UnitPosition = cp, HitChance = HitChance.VeryHigh, };
        }

        public static Vector3 PredEx(Obj_AI_Base player, float delay) 
        {
            float va = 0f;
            if (player.IsFacing(Player))
            {
                va = (50f - player.BoundingRadius);
            }
            else
            {
                va = -(100f - player.BoundingRadius);
            }
            var dis = delay * player.MoveSpeed + va;
            var path = player.GetWaypoints();
            for (var i = 0; i < path.Count - 1; i++)
            {
                var a = path[i];
                var b = path[i + 1];
                var d = a.Distance(b);

                if (d < dis)
                {
                    dis -= d;
                }
                else
                {
                    return (a + dis * (b - a).Normalized()).To3D();
                }
            }
            return (path[path.Count - 1]).To3D();
        }

        public static Vector3 FastPrediction(Vector2 from, Obj_AI_Base unit, int delay, int speed)
        {
            var tDelay = delay / 1000f + (from.Distance(unit) / speed);
            var d = tDelay * unit.MoveSpeed;
            var path = unit.GetWaypoints();

            if (path.GetPathLength() > d)
            {
                return PredictedPos = path.CutPath((int)d)[0].To3D();
            }

            return PredictedPos = path[path.Count - 1].To3D();

        }
    }
}
