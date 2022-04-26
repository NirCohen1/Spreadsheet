using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

class SharableSpreadsheet
{
    string[,] spreadsheet;
    int m_rows;
    int m_cols;
    int m_users;
    Mutex saving;
    SemaphoreSlim current_users;
    List<SemaphoreSlim> semaphore_rows;
    List<SemaphoreSlim> semaphore_cols;
    List<Mutex> rows_mutex;
    List<Mutex> cols_mutex;

    private static Mutex exchange = new Mutex();

    public SharableSpreadsheet(int nRows, int nCols)
    {
        // construct a nRows*nCols spreadsheet
        spreadsheet = new string[nRows, nCols];
        m_rows = nRows;
        m_cols = nCols;
        m_users = nCols * nRows;
        if (m_users <= 5)
            m_users = 5;

        rows_mutex = new List<Mutex>();
        cols_mutex = new List<Mutex>();
        saving = new Mutex();

        current_users = new SemaphoreSlim(m_users, m_users);
        semaphore_rows = new List<SemaphoreSlim>();
        semaphore_cols = new List<SemaphoreSlim>();

        int n = 0;
        for (int i = 0; i < m_rows; i++)
            for (int j = 0; j < m_cols; j++)
                spreadsheet[i, j] = "testcell" + (n++).ToString();

        for (int i = 0; i < m_rows; i++)
        {
            rows_mutex.Add(new Mutex());
            semaphore_rows.Add(new SemaphoreSlim(m_users, m_users));
        }
        for (int i = 0; i < m_cols; i++)
        {
            cols_mutex.Add(new Mutex());
            semaphore_cols.Add(new SemaphoreSlim(m_users, m_users));
        }
    }

    //reader - block writer
    public String getCell(int row, int col)
    {
        if (row >= m_rows || col >= m_cols || row < 0 || col < 0)
            return null;

        current_users.Wait();
        semaphore_rows[row].Wait();
        semaphore_cols[col].Wait();

        // return the string at [row,col]
        String ret = spreadsheet[row, col];

        semaphore_rows[row].Release();
        semaphore_cols[col].Release();
        current_users.Release();

        return ret;
    }

    //writer - block write, read
    public bool setCell(int row, int col, String str)
    {
        if (row >= m_rows || col >= m_cols || row < 0 || col < 0)
            return false;

        //one more user

        //lock writing, exchange and searching to rows and cols
        //exchange.WaitOne();
        current_users.Wait();
        rows_mutex[row].WaitOne();
        cols_mutex[col].WaitOne();
        //exchange.ReleaseMutex();

        //waiting for all readers to finish. take whatever he can from the semaphore, if he reach to its end(m_users) it's mean that there are no readers.
        for (int i = 0; i < m_users; i++)
            semaphore_rows[row].Wait();
        for (int i = 0; i < m_users; i++)
            semaphore_cols[col].Wait();

        // set the string at [row,col]
        spreadsheet[row, col] = str;

        semaphore_rows[row].Release(m_users);
        semaphore_cols[col].Release(m_users);

        cols_mutex[col].ReleaseMutex();
        rows_mutex[row].ReleaseMutex();
        current_users.Release();
        return true;
    }

    //reader - block writer
    public bool searchString(String str, ref int row, ref int col)
    {
        current_users.Wait();
        // search the cell with string str, and return true/false accordingly.
        // return the first cell that contains the string (search from first row to the last row)
        for (int i = 0; i < m_rows; i++)
        {
            for (int j = 0; j < m_cols; j++)
            {
                rows_mutex[i].WaitOne();
                cols_mutex[j].WaitOne();
                // stores the location in row,col.
                if (spreadsheet[i, j] != null && spreadsheet[i, j].Equals(str))
                {
                    rows_mutex[i].ReleaseMutex();
                    cols_mutex[j].ReleaseMutex();
                    current_users.Release();
                    row = i;
                    col = j;
                    return true;
                }
                cols_mutex[j].ReleaseMutex();
                rows_mutex[i].ReleaseMutex();
            }
        }
        current_users.Release();
        return false;
    }

    //writer - block write, read
    public bool exchangeRows(int row1, int row2)
    {
        if (row1 >= m_rows || row2 >= m_rows || row1 < 0 || row2 < 0 || row1 == row2)
            return false;

        //avoid searching and other exchange in spreadsheet
        exchange.WaitOne();
        current_users.Wait();
        //avoid other exchange
        //lock writing to rows(setcell)
        rows_mutex[row1].WaitOne();
        rows_mutex[row2].WaitOne();

        //for (int i = 0; i < m_users; i++)
        //    semaphore_rows[row1].Wait();
        //for (int i = 0; i < m_users; i++)
        //    semaphore_rows[row2].Wait();

        // exchange the content of row1 and row2
        string[] temp = new string[m_cols];
        for (int i = 0; i < m_cols; i++)
        {
            temp[i] = spreadsheet[row1, i];
            spreadsheet[row1, i] = spreadsheet[row2, i];
            spreadsheet[row2, i] = temp[i];
        }

        //semaphore_rows[row1].Release(m_users);
        //semaphore_rows[row2].Release(m_users);

        rows_mutex[row1].ReleaseMutex();
        rows_mutex[row2].ReleaseMutex();

        exchange.ReleaseMutex();
        current_users.Release();

        return true;
    }

    //writer - block write, read
    public bool exchangeCols(int col1, int col2)
    {
        if (col1 >= m_cols || col2 >= m_cols || col1 < 0 || col2 < 0 || col1 == col2)
            return false;
        //avoid searching in spreadsheet && avoid other exchange
        exchange.WaitOne();
        current_users.Wait();

        //lock writing to columns
        cols_mutex[col1].WaitOne();
        cols_mutex[col2].WaitOne();

        ////acquire all the semaphore from readers
        //for (int i = 0; i < m_users; i++)
        //{
        //    semaphore_cols[col1].Wait();
        //    semaphore_cols[col2].Wait();
        //}

        // exchange the content of col1 and col2
        string[] temp = new string[m_rows];
        for (int i = 0; i < m_rows; i++)
        {
            temp[i] = spreadsheet[i, col1];
            spreadsheet[i, col1] = spreadsheet[i, col2];
            spreadsheet[i, col2] = temp[i];
        }

        //semaphore_cols[col1].Release(m_users);
        //semaphore_cols[col2].Release(m_users);

        exchange.ReleaseMutex();

        cols_mutex[col1].ReleaseMutex();
        cols_mutex[col2].ReleaseMutex();
        current_users.Release();

        return true;
    }

    //reader - block writer
    public bool searchInRow(int row, String str, ref int col)
    {
        if (row < 0 || row >= m_rows)
            return false;

        current_users.Wait();
        // perform search in specific row
        for (int i = 0; i < m_cols; i++)
        {
            rows_mutex[row].WaitOne();
            cols_mutex[i].WaitOne();
            if (spreadsheet[row, i] != null && spreadsheet[row, i].Equals(str))
            {
                col = i;
                rows_mutex[row].ReleaseMutex();
                cols_mutex[i].ReleaseMutex();
                current_users.Release();
                return true;
            }
            cols_mutex[i].ReleaseMutex();
            rows_mutex[row].ReleaseMutex();
        }
        current_users.Release();
        return false;
    }

    //reader - block writer
    public bool searchInCol(int col, String str, ref int row)
    {
        if (col < 0 || col >= m_cols)
            return false;
        //exchange.WaitOne();
        current_users.Wait();
        //exchange.ReleaseMutex();
        // perform search in specific col
        for (int i = 0; i < m_rows; i++)
        {
            rows_mutex[i].WaitOne();
            if (spreadsheet[i, col] != null && spreadsheet[i, col].Equals(str))
            {
                row = i;
                current_users.Release();
                rows_mutex[i].ReleaseMutex();
                return true;
            }
            rows_mutex[i].ReleaseMutex();
        }
        current_users.Release();
        return false;
    }

    //reader - block writer
    public bool searchInRange(int col1, int col2, int row1, int row2, String str, ref int row, ref int col)
    {
        if (col1 < 0 || col1 > m_cols - 1 || col2 < 0 || col2 > m_cols - 1)
            return false;
        if (row1 < 0 || row1 > m_rows - 1 || row2 < 0 || row2 > m_rows - 1)
            return false;
        if (row1 < row2 || col1 < col2)
            return false;


        current_users.Wait();
        // perform search within spesific range: [row1:row2,col1:col2] 
        //includes col1,col2,row1,row2
        for (int i = 0; i < row2 - row1 + 1; i++)
        {
            semaphore_rows[i].Wait();
            for (int j = 0; j < col2 - col1 + 1; j++)
            {
                semaphore_cols[j].Wait();
                if (spreadsheet[i, j].Equals(str))
                {
                    row = i;
                    col = j;
                    semaphore_cols[j].Release();
                    semaphore_rows[i].Release();
                    current_users.Release();
                    return true;
                }
                semaphore_cols[j].Release();
            }
            semaphore_rows[i].Release();
        }
        current_users.Release();
        return false;
    }

    //writer - block write, read
    public bool addRow(int row1)
    {
        if (row1 >= m_rows || row1 < 0)
            return false;

        exchange.WaitOne();
        for (int i = 0; i < m_users; i++)
            current_users.Wait();

        row1++;
        //add a row after row1
        string[,] new_sheet = new string[m_rows + 1, m_cols];

        for (int i = 0; i < m_cols; i++)
        {
            new_sheet[row1, i] = "new cell";
            for (int j = 0; j < row1; j++)
                new_sheet[j, i] = spreadsheet[j, i];
            for (int j = row1 + 1; j < m_rows + 1; j++)
                new_sheet[j, i] = spreadsheet[j - 1, i];
        }

        //lock

        rows_mutex.Add(new Mutex());
        semaphore_rows.Add(new SemaphoreSlim(m_users, m_users));

        spreadsheet = new_sheet;
        m_rows++;
        current_users.Release(m_users);

        exchange.ReleaseMutex();
        //unlock

        return true;
    }

    //writer - block write, read
    public bool addCol(int col1)
    {
        //add a column after col1
        if (col1 >= m_cols || col1 < 0)
            return false;

        exchange.WaitOne();
        for (int i = 0; i < m_users; i++)
            current_users.Wait();

        string[,] new_sheet = new string[m_rows, m_cols + 1];

        col1++;

        for (int i = 0; i < m_rows; i++)
        {
            new_sheet[i, col1] = "new cell";
            for (int j = 0; j < col1; j++)
                new_sheet[i, j] = spreadsheet[i, j];
            for (int j = col1 + 1; j < m_cols; j++)
                new_sheet[i, j] = spreadsheet[i, j - 1];
        }

        cols_mutex.Add(new Mutex());
        semaphore_cols.Add(new SemaphoreSlim(m_users, m_users));
        spreadsheet = new_sheet;
        m_cols++;
        exchange.ReleaseMutex();

        current_users.Release(m_users);
        return true;
    }

    //reader - block writer of add col/row
    public void getSize(ref int nRows, ref int nCols)
    {
        // return the size of the spreadsheet in nRows, nCols
        nRows = m_rows;
        nCols = m_cols;
    }

    public bool setConcurrentSearchLimit(int nUsers)
    {
        // this function aims to limit the number of users that can perform the search operations concurrently.
        // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
        // In this case additional search operations will wait for existing search to finish.

        if (m_users <= nUsers)
            return false;
        exchange.WaitOne();
        for (int i = 0; i < m_users; i++)
            current_users.Wait();

        m_users = nUsers;
        rows_mutex = new List<Mutex>();
        cols_mutex = new List<Mutex>();
        semaphore_rows = new List<SemaphoreSlim>();
        semaphore_cols = new List<SemaphoreSlim>();

        for (int i = 0; i < m_rows; i++)
        {
            rows_mutex.Add(new Mutex());
            semaphore_rows.Add(new SemaphoreSlim(m_users, m_users));
        }
        for (int i = 0; i < m_cols; i++)
        {
            cols_mutex.Add(new Mutex());
            semaphore_cols.Add(new SemaphoreSlim(m_users, m_users));
        }

        current_users = new SemaphoreSlim(nUsers, nUsers);
        exchange.ReleaseMutex();


        return true;
    }

    public bool save(String fileName)
    {
        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
        exchange.WaitOne();
        saving.WaitOne();
        current_users.Wait();
        exchange.ReleaseMutex();
        try
        {
            using (TextWriter tw = new StreamWriter("spreadsheet.dat"))
            {
                for (int i = 0; i < m_rows; i++)
                {
                    for (int j = 0; j < m_cols; j++)
                        tw.Write(spreadsheet[i, j] + " ");
                    tw.WriteLine();
                }
            }
        }
        catch
        {
            current_users.Release();
            saving.ReleaseMutex();
            return false;
        }
        current_users.Release();
        saving.ReleaseMutex();

        return true;
    }
    public bool load(String fileName)
    {
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data
        int i = 0;
        try
        {
            List<List<string>> matrix = new List<List<string>>();
            foreach (string line in File.ReadLines(fileName))
            {
                matrix.Add(new List<string>());
                string[] words = line.Split(' ');
                foreach (string s in words)
                    matrix[i].Add(s);
                i++;
            }

            SharableSpreadsheet spreadsheeta = new SharableSpreadsheet(matrix.Count, matrix[0].Count);
            for (int k = 0; k < matrix.Count; k++)
                for (int m = 0; m < matrix[0].Count; m++)
                    spreadsheeta.spreadsheet[k, m] = matrix[k][m];


            //block all users
            for (int k = 0; k < m_users; k++)
                current_users.Wait();

            spreadsheet = spreadsheeta.spreadsheet;
            m_rows = spreadsheeta.m_rows;
            m_cols = spreadsheeta.m_cols;
            m_users = m_rows * m_cols;

            rows_mutex = spreadsheeta.rows_mutex;
            cols_mutex = spreadsheeta.cols_mutex;
            semaphore_rows = spreadsheeta.semaphore_rows;
            semaphore_cols = spreadsheeta.semaphore_cols;

            //new semaphore - releasing not needed
            current_users = spreadsheeta.current_users;
        }
        catch
        {
            return false;
        }

        return true;
    }

}