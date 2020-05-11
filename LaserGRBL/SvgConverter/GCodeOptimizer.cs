using System;
using System.Collections.Generic;
using System.Windows;
using LaserGRBL.UserControls;

// This class implements a greedy and grid based nearest neighbour path finding algorithm.
// 
// - The route is far from optimal, but better than nothing.
// - The gready version can take a very long time for large files, so pretty much useless
// - NB: The GCodeOptimizer has been specifical designed to use the GCode output from the GCodeFromSVG
//   and will behave unpredictably using gcode from another source
namespace LaserGRBL.SvgConverter {


    public class GCodeOptimizer {

        static readonly int GRID_SIZE = 10;
        static readonly int SCAN_RANGE = 2;

        private class GrblCommandGroup : List<GrblCommand> {

            private Point mStart = new Point();
            private Point mEnd = new Point();

            public void Add(Point point, GrblCommand command) {

                if (Count == 0) {
                    mStart = new Point(point.X, point.Y);
                }
                mEnd = new Point(point.X, point.Y);

                base.Add(command);
            }

            public Point Start { get { return mStart; } }
            public Point End { get { return mEnd; } }
        }

        private class GrblCommandGrid : Dictionary<Point, List<GrblCommandGroup>> {

            public void Add(GrblCommandGroup group) {

                var key = new Point((int)Math.Floor(group.Start.X / GRID_SIZE), (int)Math.Floor(group.Start.Y / GRID_SIZE));
                if (!ContainsKey(key)) {

                    Add(key, new List<GrblCommandGroup>());
                }
                this[key].Add(group);
            }

            public void Remove(GrblCommandGroup group) {

                var key = new Point((int)Math.Floor(group.Start.X / GRID_SIZE), (int)Math.Floor(group.Start.Y / GRID_SIZE));
                if (ContainsKey(key)) {

                    this[key].Remove(group);
                    if (this[key].Count == 0) {

                        this.Remove(key);
                    }
                }
            }
        }

        // Distance Function (without the Sqrt)
        private static double DistanceSqr(Point p1, Point p2) {

            return Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
        }

        public static void AddImplicitMCodes(List<GrblCommand> commands) {

            var list = new List<GrblCommand>();
            var spb = new GrblCommand.StatePositionBuilder();

            float S = 0;
            foreach (GrblCommand cmd in commands) {

                cmd.BuildHelper();
                spb.AnalyzeCommand(cmd, false);

                float lastS = S;
                if (cmd.S != null) {

                    S = (float)cmd.S.Number;
                }

                if (cmd.M == null) {

                    if (lastS != S) {

                        list.Add(new GrblCommand(spb.LaserBurning ? "M3 S" + S.ToString() : "M5"));
                    }
                }
                list.Add(cmd);
            }

            commands.Clear();
            foreach (GrblCommand cmd in list) {

                commands.Add(cmd);
            }
        }

        public static void AddMCodeZMoves(List<GrblCommand> commands) {
            
            var spb = new GrblCommand.StatePositionBuilder();
            var list = new List<GrblCommand>();
            foreach (GrblCommand cmd in commands) {

                cmd.BuildHelper();
                spb.AnalyzeCommand(cmd, false);

                list.Add(cmd);

                if (cmd.M != null) {

                    switch ((int)cmd.M.Number) {

                        case 3:
                            list.Add(new GrblCommand("G0 Z0"));
                            break;
                        case 5:
                            list.Add(new GrblCommand("G0 Z1"));
                            break;                        
                    }                    
                }
            }

            commands.Clear();
            foreach (GrblCommand cmd in list) {

                commands.Add(cmd);
            }
        }

        // Group GrblCommands into groups (paths) where the laser is burning
        private static List<GrblCommandGroup> BuildGroups(List<GrblCommand> commands) {

            var groups = new List<GrblCommandGroup>();

            var spb = new GrblCommand.StatePositionBuilder();
            var group = new GrblCommandGroup();
            foreach (GrblCommand cmd in commands) {

                cmd.BuildHelper();
                spb.AnalyzeCommand(cmd, false);
                Point pos = new Point((float)spb.X.Number, (float)spb.Y.Number);

                group.Add(pos, cmd);
                if (cmd.M != null && (int) cmd.M.Number == 5) {

                         groups.Add(group);
                         group = new GrblCommandGroup();
                }
            }

            if (group.Count > 0) {
                groups.Add(group);
            }

            return groups;
        }

        //Perform Grid Based Nearest Neighbout Optimization
        public static void PerformGridBasedNearestNeighboutOptimization(List<GrblCommand> commands) {

            //Performance Timer
            var timer = System.Diagnostics.Stopwatch.StartNew();

            // Build Groups
            ProgressDialog.Text = "Optimizing - Generating Paths";

            var groups = BuildGroups(commands);

            // Build the Grid
            ProgressDialog.Text = "Optimizing - Generating Grid";

            var grid = new GrblCommandGrid();
            foreach (var group in groups) {

                grid.Add(group);
            }

            //Optimize
            ProgressDialog.Text = "Optimizing - Grid Based Nearest Neigbour";

            commands.Clear();
            Point current = new Point();
            int hits = 0, misses = 0;
            while (grid.Count > 0) {

                GrblCommandGroup best = null;
                double bestDistance = 0;

                // Scan Grid for the Nearest Neighbour inside range            
                var gridX = (int)Math.Floor(current.X / GRID_SIZE);
                var gridY = (int)Math.Floor(current.Y / GRID_SIZE);

                for (int x = Math.Max(0, gridX - SCAN_RANGE); x < gridX + SCAN_RANGE; x++) {

                    for (int y = Math.Max(0, gridY - SCAN_RANGE); y < gridY + SCAN_RANGE; y++) {

                        var key = new Point(x, y);
                        if (grid.ContainsKey(key)) {

                            foreach (GrblCommandGroup path in grid[key]) {

                                //double distance = Point.Subtract(current, path.Start).Length;
                                double distance = DistanceSqr(current, path.Start);
                                if (best == null || distance < bestDistance) {

                                    best = path;
                                    bestDistance = distance;
                                }
                            }
                        }
                    }
                }

                //If nothing is found, scan the whole grid
                if (best == null) {

                    misses++;
                    foreach (var item in grid) {

                        foreach (GrblCommandGroup path in item.Value) {

                            double distance = DistanceSqr(current, path.Start);
                            if (best == null || distance < bestDistance) {

                                best = path;
                                bestDistance = distance;
                            }
                        }
                    }
                } else {
                    hits++;
                }

                // Remember last position
                current = new Point(best.End.X, best.End.Y);

                //Remove from Grid
                grid.Remove(best);

                //Add command to move to starting position (if not the first command)
                if (commands.Count > 0) {

                    commands.Add(new GrblCommand("G0 X" + best.Start.X.ToString() + " Y" + best.Start.Y.ToString()));
                }

                //Add the commands to result
                foreach (GrblCommand cmd in best) {

                    commands.Add(cmd);
                }
            }

            //Done
            timer.Stop();
            ProgressDialog.Text = "Optimizing - Elapsed: " + timer.ElapsedMilliseconds.ToString() + "ms";
            Console.WriteLine("GCode Optimization Completed. Grid Hits:" + hits.ToString() + " Misses: " + misses.ToString() + " Elapsed: " + timer.ElapsedMilliseconds.ToString() + "ms");
        }

        //Perform Greedy Nearest Neighbout Optimization
        public static void PerformGreedyNearestNeighboutOptimization(List<GrblCommand> commands) {

            // Performance Timer
            var timer = System.Diagnostics.Stopwatch.StartNew();

            // Build Groups
            ProgressDialog.Text = "Optimizing - Generating Paths";

            var groups = BuildGroups(commands);

            // Greedy Nearest Neigbour
            ProgressDialog.Text = "Optimizing - Greedy Nearest Neigbour";

            commands.Clear();
            Point current = new Point();
            while (groups.Count > 0) {

                // Iterate over all groups (paths) and find the closest to the current position
                GrblCommandGroup best = null;
                double bestDistance = 0;

                foreach (GrblCommandGroup group in groups) {

                    double distance = DistanceSqr(current, group.Start);
                    if (best == null || distance < bestDistance) {

                        best = group;
                        bestDistance = distance;
                    }
                }

                // Remember last position
                current = new Point(best.End.X, best.End.Y);

                //Remove from Groups
                groups.Remove(best);

                //Add command to move to starting position (if not the first command)
                if (commands.Count > 0) {

                    commands.Add(new GrblCommand("G0 X" + best.Start.X.ToString() + " Y" + best.Start.Y.ToString()));
                }

                //Add the commands to result
                foreach (GrblCommand cmd in best) {

                    commands.Add(cmd);
                }
            }

            //Done
            timer.Stop();

            ProgressDialog.Text = "Optimizing - Elapsed: " + timer.ElapsedMilliseconds.ToString() + "ms";
            Console.WriteLine("GCode Optimization Completed. Elapsed: " + timer.ElapsedMilliseconds.ToString() + "ms");
        }

    }


}