using System;
using System.Threading;

namespace ConsoleApp4
{
    class Program
    {
        static SharableSpreadsheet spreadaheet;
        static string[] randoms_strings;
        static int cols, rows;

        static void Main(string[] args)
        {
            randoms_strings = new string[] { "Spread Sheet1", "testcell1", "Sheet on my life", "tester", "100 baavoda", "Sheet on me", "Barca", "June 14 respect" };
            rows = Convert.ToInt32(args[0]);
            cols = Convert.ToInt32(args[1]);
            int nThreads = Convert.ToInt32(args[2]);
            int nOperations = Convert.ToInt32(args[3]);

            //int nThreads = 5, nOperations = 100;
            //cols = 2;
            //rows = 1;

            int running = nThreads;
            AutoResetEvent done = new AutoResetEvent(false);
            ThreadPool.SetMaxThreads(nThreads, 0);
            spreadaheet = new(rows, cols);

            for (int i = 0; i < nThreads; i++)
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    GenerarateMethods(nOperations, nThreads);
                    if (0 == Interlocked.Decrement(ref running))
                        done.Set();
                });
            }
            done.WaitOne();

            //Console.WriteLine("finish");
        }

        private static int GenerarateMethods(int nOperations, int nThreads)
        {
            Random rnd = new Random();

            for (int i = 0; i < nOperations; i++)
            {
                int r = rnd.Next(0, 13);
                int index = rnd.Next(0, randoms_strings.Length);
                int row = rnd.Next(0, rows), col = rnd.Next(0, cols);
                int row1 = rnd.Next(0, rows), col1 = rnd.Next(0, cols);
                string str = randoms_strings[index];
                int x = 0, y = 0;

                switch (r)
                {
                    case 0:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        string s = spreadaheet.getCell(row, col);
                        Console.WriteLine("User[{0}]: string " + s + " found in cell [" + row.ToString() + "," + col.ToString() + "].", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 1:

                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        spreadaheet.setCell(row, col, str);
                        Console.WriteLine("User[{0}]: string " + str + " set in cell [" + row.ToString() + "," + col.ToString() + "].", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 2:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret = spreadaheet.searchString(str, ref x, ref y);
                        if (ret)
                            Console.WriteLine("User[{0}]: string " + str + " founded in cell [" + x.ToString() + "," + y.ToString() + "].", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: string " + str + " wasn't founded.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 3:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret1 = spreadaheet.exchangeRows(row, row1);
                        if (ret1)
                            Console.WriteLine("User[{0}]: row [" + row.ToString() + "] and [" + row1.ToString() + "] exchanged successfully.", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: fail to exchange rows.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 4:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret2 = spreadaheet.exchangeCols(col, col1);
                        if (ret2)
                            Console.WriteLine("User[{0}]: col [" + col.ToString() + "] and [" + col1.ToString() + "] exchanged successfully.", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: fail to exchange cols.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 5:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret3 = spreadaheet.searchInRow(row, str, ref x);
                        if (ret3)
                            Console.WriteLine("User[{0}]: string " + str + " found in cell [" + row.ToString() + "," + x.ToString() + "].", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: string " + str + " wasn't founded.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 6:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret4 = spreadaheet.searchInCol(col, randoms_strings[index], ref x);
                        if (ret4)
                            Console.WriteLine("User[{0}]: string " + str + " found in cell [" + x.ToString() + "," + col.ToString() + "].", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: string " + str + " wasn't founded.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 7:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret5 = spreadaheet.searchInRange(col, col1, row, row1, randoms_strings[index], ref x, ref y);
                        if (ret5)
                            Console.WriteLine("User[{0}]: string " + str + " found in cell [" + x.ToString() + "," + y.ToString() + "].", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: string " + str + " wasn't founded.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 8:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret6 = spreadaheet.addRow(row);
                        if (ret6)
                        {
                            rows++;
                            Console.WriteLine("User[{0}]: add new row in " + col.ToString(), Thread.CurrentThread.ManagedThreadId);
                        }
                        else
                            Console.WriteLine("User[{0}]: can't add row.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 9:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret7 = spreadaheet.addCol(col);
                        if (ret7)
                        {
                            cols++;
                            Console.WriteLine("User[{0}]: add new column in " + col.ToString(), Thread.CurrentThread.ManagedThreadId);
                        }
                        else
                            Console.WriteLine("User[{0}]: can't add column.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 10:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        spreadaheet.getSize(ref x, ref y);
                        Console.WriteLine("User[{0}]: spreadsheet size: rows: " + x.ToString() + ", columns: " + y.ToString(), Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 11:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret8 = spreadaheet.setConcurrentSearchLimit(rnd.Next(3, nThreads));
                        if (ret8)
                            Console.WriteLine("User[{0}]: max number of concurrent search operations is set.", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: can't set new max users.", Thread.CurrentThread.ManagedThreadId);
                        break;
                    case 12:
                        Console.WriteLine("User[{0}] start " + r.ToString(), Thread.CurrentThread.ManagedThreadId);
                        bool ret9 = spreadaheet.save("_tempfile.txt");
                        if (ret9)
                            Console.WriteLine("User[{0}]: save to _tempfile.txt.", Thread.CurrentThread.ManagedThreadId);
                        else
                            Console.WriteLine("User[{0}]: can't save.", Thread.CurrentThread.ManagedThreadId);
                        break;
                }
                //Make sure that you randomly choose the operation and the data you perform.
                Thread.Sleep(100);
            }
            return 0;
        }
    }
}
