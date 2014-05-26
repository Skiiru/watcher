using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Data
{
    public enum Type
    {
        process,history,activity,files
    }
    [Table]
    public class Data
    {
        [Column(IsPrimaryKey=true,IsDbGenerated= true)]
        public int id;
        [Column]
        public string data;
        [Column]
        public DateTime time;
        [Column]
        public Type type;
        [Column]
        public int user_id;

        public Data(string d,DateTime dt, Type t,int id)
        {
            data = d;
            type = t;
            user_id = id;
            time = DateTime.Now;
        }
    }
    public class dbData : DataContext
    {
        public dbData(string conn):base(conn)
        { }
        public System.Data.Linq.Table<Data> Data
        {
            get { return this.GetTable<Data>(); }
        }
        public void Check(string conn)
        {
           dbData db = new dbData(conn);
           
        }
        public void Add(Data d,string c)
        {
            dbData db = new dbData(c);
            db.Data.InsertOnSubmit(d);
            db.SubmitChanges();
        }
    }
}
