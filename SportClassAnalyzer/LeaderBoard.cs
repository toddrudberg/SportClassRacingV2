using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportClassAnalyzer
{
    public class LeaderBoard
    {
        public List<cRacerStatus> _racerStatuses { get; set; } = new();
        public string GetDisplayText()
        {
            var sb = new StringBuilder();

            // Use headers that match the column widths
            sb.AppendLine("Pos | Name       |  Speed | Interval | Laps");

            var sorted = _racerStatuses.OrderBy(r => r.Position).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var r = sorted[i];
                string intervalText = i == 0 ? "LEAD    " : $"+{r.TimeToLeader,6:0.0}s";

                sb.AppendLine($"{r.Position,3} | {r.Name,-10} | {r.Speed,5:0}  | {intervalText} | {r.LapsCompleted,4:0.0}");
            }

            return sb.ToString();
        }

    }

    public class cRacerStatus
    {
        public string Name { get; set; }
        public int Position { get; set; }
        public float Speed { get; set; }
        public float TimeToLeader { get; set; }
        public float TimeToNext { get; set; }
        public float LapsCompleted { get; set; }
    }
}
