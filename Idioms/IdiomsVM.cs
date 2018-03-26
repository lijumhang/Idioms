using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Idioms
{
    class IdiomsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string NewProp)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(NewProp));
        }

        public class Idiom
        {
            public string Original;
            public string Translation;
            public Idiom(string orig, string trans)
            {
                Original = orig;
                Translation = trans;
            }
        }

        private Idiom _idiomText;
        public Idiom IdiomText
        {
            get { return _idiomText; }
            set
            {
                _idiomText = value;
                OnPropertyChanged("IdiomText");
            }
        }
        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public NewCommand Show { get; set; } = new NewCommand();
        public NewCommand GetPrevious { get; set; } = new NewCommand();
        public NewCommand GetNext { get; set; } = new NewCommand();

        private bool _prevEnable;
        public bool PrevEnable
        {
            get { return _prevEnable; }
            set
            {
                _prevEnable = value;
                OnPropertyChanged("PrevEnable");
            }
        }
        private bool _nextEnable;
        public bool NextEnable
        {
            get { return _nextEnable; }
            set
            {
                _nextEnable = value;
                OnPropertyChanged("NextEnable");
            }
        }

        List<Idiom> IdiomsList = new List<Idiom>();

        public IdiomsVM()
        {
            Show.Func = LetShow;
            GetPrevious.Func = Previous;
            GetNext.Func = Next;

            PrevEnable = false;
            NextEnable = true;

            using (SqlConnection db = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                SqlCommand NewQuery = new SqlCommand("select Original, Translation from Idioms", db);
                SqlDataReader reader;
                try
                {
                    db.Open();
                    reader = NewQuery.ExecuteReader();
                    SqlDataAdapter Adapter = new SqlDataAdapter(NewQuery);
                    DataTable table = new DataTable("Idioms");
                    db.Close();
                    Adapter.Fill(table);
                    foreach (DataRow row in table.Rows)
                    {
                        IdiomsList.Add(new Idiom((string)row.ItemArray[0], (string)row.ItemArray[1]));
                    }
                }
                catch
                {
                    MessageBox.Show("Подключение к базе данных не доступно");
                    return;
                }
            }
            IdiomText = IdiomsList[0];
            Text = IdiomText.Original;
        }

        public void LetShow(object something)
        {
            Text = IdiomText.Original + "\n\n" + IdiomText.Translation;
        }

        public void Previous(object something)
        {
            NextEnable = true;
            PrevEnable = true;
            IdiomText = IdiomsList[IdiomsList.IndexOf(IdiomText) - 1];
            Text = IdiomText.Original;
            if (IdiomText == IdiomsList[0])
                PrevEnable = false;
        }

        public void Next(object something)
        {
            PrevEnable = true;
            NextEnable = true;
            IdiomText = IdiomsList[IdiomsList.IndexOf(IdiomText) + 1];
            Text = IdiomText.Original;
            if (IdiomText == IdiomsList[IdiomsList.Count - 1])
                NextEnable = false;
        }
    }

    public class NewCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public Action<object> Func { get; set; }
        public bool CanExecute(object parameter) { return true; }
        public void Execute(object parameter)
        {
            Func(parameter);
        }
    }
}
