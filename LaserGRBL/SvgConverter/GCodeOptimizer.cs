using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using System.Threading;
using LaserGRBL.UserControls;

// This class implements a grid-based nearest neighbour algorithm
//
// The GCodeOptimizer has been specifical designed to use the GCode output from the GCodeFromSVG
// and will behave unpredictably using gcode from another source
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
                //Add(key, group);
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





        private class GCodePath {

            private Point start = new Point();
            private Point end = new Point();

            private List<GrblCommand> mCommands = new List<GrblCommand>();

            public void addCommand(GrblCommand cmd, Point pos) {

                if (mCommands.Count == 0) {

                    this.start = new Point(pos.X, pos.Y);
                }
                this.end = new Point(pos.X, pos.Y);
                mCommands.Add(cmd);
            }

            public List<GrblCommand> Commands { get { return mCommands; } }
            public Point Start { get { return start; } }
            public Point End { get { return end; } }
        }

        private class GCodePointGrid {

            private Dictionary<int, Dictionary<int, List<GCodePath>>> grid = new Dictionary<int, Dictionary<int, List<GCodePath>>>();

            public void addPath(GCodePath path) {

                int gridX = (int)Math.Floor(path.Start.X / GRID_SIZE);
                int gridY = (int)Math.Floor(path.Start.Y / GRID_SIZE);

                if (!grid.ContainsKey(gridX)) {

                    grid.Add(gridX, new Dictionary<int, List<GCodePath>>());
                }
                if (!grid[gridX].ContainsKey(gridY)) {

                    grid[gridX].Add(gridY, new List<GCodePath>());
                }

                grid[gridX][gridY].Add(path);
            }

            public void removePath(GCodePath path) {

                int gridX = (int)Math.Floor(path.Start.X / GRID_SIZE);
                int gridY = (int)Math.Floor(path.Start.Y / GRID_SIZE);
                if (grid.ContainsKey(gridX)) {

                    if (grid[gridX].ContainsKey(gridY)) {
                        grid[gridX][gridY].Remove(path);

                        if (grid[gridX][gridY].Count == 0) {

                            grid[gridX].Remove(gridY);
                        }
                    }

                    if (grid[gridX].Count == 0) {

                        grid.Remove(gridX);
                    }
                }

            }

            public Dictionary<int, Dictionary<int, List<GCodePath>>> Paths { get { return grid; } }

            public List<GCodePath> getCell2(int gridX, int gridY) {

                if (grid.ContainsKey(gridX)) {

                    if (grid[gridX].ContainsKey(gridY)) {

                        return grid[gridX][gridY];
                    }
                }
                return null;
            }

        }

        private List<GrblCommand> commands;

        public GCodeOptimizer(List<GrblCommand> commands) {

            this.commands = commands;
        }


        private static double DistanceQuick(Point p1, GCodePath p2) {
            // distance will this or less
            double deltaX = Math.Abs(p2.Start.X - p1.X);
            double deltaY = Math.Abs(p2.Start.Y - p1.Y);
            return deltaX > deltaY ? deltaX : deltaY;
        }

        private static double DistanceSqr(Point p1, Point p2) {

            return Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
        }


        private List<GrblCommandGroup> BuildGroups() {

            var groups = new List<GrblCommandGroup>();

            var spb = new GrblCommand.StatePositionBuilder();
            var group = new GrblCommandGroup();
            foreach (GrblCommand cmd in commands) {

                cmd.BuildHelper();
                spb.AnalyzeCommand(cmd, false);
                Point pos = new Point((float)spb.X.Number, (float)spb.Y.Number);

                group.Add(pos, cmd);
                if (!spb.LaserBurning) {

                    groups.Add(group);
                    group = new GrblCommandGroup();
                }
            }

            if (group.Count > 0) {
                groups.Add(group);
            }

            return groups;
        }



        public void run() {


            process2();
        }

        public void process2() {


            //Performance Timer
            var timer = System.Diagnostics.Stopwatch.StartNew();

            // Build Groups
            ProgressDialog.Text = "Optimizing - Generating Paths";

            var groups = BuildGroups();

            // Build the Grid
            ProgressDialog.Text = "Optimizing - Generating Grid";
            var grid = new GrblCommandGrid();
            foreach (var group in groups) {

                grid.Add(group);
            }


            // ProgressDialog.Maximum = groups.Count();

            //Optimize
            ProgressDialog.Text = "Optimizing - Nearest Neigbour";
            commands.Clear();
            Point current = new Point();
            int hits = 0, misses = 0;
            while (grid.Count > 0) {

                //  ProgressDialog.Progress++;

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

                    this.commands.Add(cmd);
                }
            }

            //Done
            timer.Stop();
            ProgressDialog.Text = "Optimizing - Elapsed: " + timer.ElapsedMilliseconds.ToString() + "ms";
            Console.WriteLine("Optmization - Complete. Elapsed: " + timer.ElapsedMilliseconds.ToString() + "ms");


        }

        public void process() {






            GrblCommand.StatePositionBuilder spb = new GrblCommand.StatePositionBuilder();

            List<GCodePath> groups = new List<GCodePath>();
            GCodePointGrid grid = new GCodePointGrid();

            GCodePath currentPath = new GCodePath();
            bool wasBurning = false;
            foreach (GrblCommand cmd in commands) {

                cmd.BuildHelper();
                spb.AnalyzeCommand(cmd, false);

                Point pos = new Point((float)spb.X.Number, (float)spb.Y.Number);
                currentPath.addCommand(cmd, pos);

                if (!spb.LaserBurning) {

                    if (wasBurning) {

                        currentPath.addCommand(new GrblCommand("G0 Z1"), pos);
                        wasBurning = false;
                    }

                    groups.Add(currentPath);
                    grid.addPath(currentPath);
                    currentPath = new GCodePath();
                } else {

                    if (!wasBurning) {

                        currentPath.addCommand(new GrblCommand("G0 Z0"), pos);
                        wasBurning = true;
                    }

                }
            }
            if (currentPath.Commands.Count > 0) {

                groups.Add(currentPath);
                grid.addPath(currentPath);
            }
            commands.Clear();

            //groups.Sort(dele)

            Point position = new Point(0, 0);

            while (groups.Count > 0) {

                //var best = groups.OrderByDescending(_path => DistanceQuick(position, _path)).Last();
                var best = groups.OrderByDescending(_path => -Point.Subtract(position, _path.Start).Length).First();
                //Point.Subtract(current, path.Start).Length;

                groups.Remove(best);
                position = new Point(best.End.X, best.End.Y);

                if (this.commands.Count > 0) {

                    this.commands.Add(new GrblCommand("G0 X" + best.Start.X.ToString() + " Y" + best.Start.Y.ToString()));
                }

                foreach (GrblCommand cmd in best.Commands) {

                    this.commands.Add(cmd);
                }

            }



            //var closest = entities.Something.OrderBy(x => 12742 * SqlFunctions.Asin(SqlFunctions.SquareRoot(SqlFunctions.Sin(((SqlFunctions.Pi() / 180) * (x.Latitude - startPoint.Latitude)) / 2) * SqlFunctions.Sin(((SqlFunctions.Pi() / 180) * (x.Latitude - startPoint.Latitude)) / 2) +
            //                       SqlFunctions.Cos((SqlFunctions.Pi() / 180) * startPoint.Latitude) * SqlFunctions.Cos((SqlFunctions.Pi() / 180) * (x.Latitude)) *
            //                      SqlFunctions.Sin(((SqlFunctions.Pi() / 180) * (x.Longitude - startPoint.Longitude)) / 2) * SqlFunctions.Sin(((SqlFunctions.Pi() / 180) * (x.Longitude - startPoint.Longitude)) / 2)))).Take(5);


            return;
            //var points = new List<decimal>();

            //points.OrderByDescending

            //  IEnumerable<decimal> test = points.order;



            Point current = new Point(0, 0);

            List<GCodePath> optimized = new List<GCodePath>();

            int gridFound = 0;
            int gridNotFound = 0;
            //while (groups.Count > 0) {
            while (grid.Paths.Count > 0) {

                int gridX = (int)Math.Floor(current.X / GRID_SIZE);
                int gridY = (int)Math.Floor(current.Y / GRID_SIZE);

                GCodePath best = null;
                double bestDistance = 0;
                for (int x = Math.Max(0, gridX - SCAN_RANGE); x < gridX + SCAN_RANGE; x++) {

                    for (int y = Math.Max(0, gridY - SCAN_RANGE); y < gridY + SCAN_RANGE; y++) {

                        List<GCodePath> cell = grid.getCell2(x, y);
                        if (cell != null) {

                            foreach (GCodePath path in cell) {

                                double distance = Point.Subtract(current, path.Start).Length;
                                if (best == null || distance < bestDistance) {

                                    best = path;
                                    bestDistance = distance;
                                }
                            }
                        }
                    }
                }


                if (best != null) {

                    gridFound++;
                } else {

                    gridNotFound++;

                    foreach (GCodePath path in groups) {

                        double distance = Point.Subtract(current, path.Start).Length;
                        if (best == null || distance < bestDistance) {

                            best = path;
                            bestDistance = distance;
                        }
                    }
                }

                //  break;
                groups.Remove(best);
                grid.removePath(best);
                current = new Point(best.End.X, best.End.Y);





                if (this.commands.Count > 0) {

                    this.commands.Add(new GrblCommand("G0 X" + best.Start.X.ToString() + " Y" + best.Start.Y.ToString()));
                }

                foreach (GrblCommand cmd in best.Commands) {

                    this.commands.Add(cmd);
                }


            }


            // public Dictionary<int, Dictionary<int, List<GCodePath>>> Paths { get { return grid; } }

            //
            foreach (var row in grid.Paths) {

                foreach (var column in row.Value) {

                    foreach (GCodePath best in column.Value) {

                        if (this.commands.Count > 0) {

                            this.commands.Add(new GrblCommand("G0 X" + best.Start.X.ToString() + " Y" + best.Start.Y.ToString()));
                        }

                        foreach (GrblCommand cmd in best.Commands) {

                            this.commands.Add(cmd);
                        }

                    }
                }
            }

            return;

            foreach (GCodePath best in groups) {

                grid.removePath(best);
                current = new Point(best.End.X, best.End.Y);


                if (this.commands.Count > 0) {

                    this.commands.Add(new GrblCommand("G0 X" + best.Start.X.ToString() + " Y" + best.Start.Y.ToString()));
                }

                foreach (GrblCommand cmd in best.Commands) {

                    this.commands.Add(cmd);
                }
            }

            //MessageBox.Show(grid.Paths.Count.ToString());
            MessageBox.Show("Found: " + gridFound.ToString() + " Not: " + gridNotFound.ToString());





        }

    }


}