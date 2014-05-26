using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace WCF
{
    [ServiceContract] // Говорим WCF что это интерфейс для запросов сервису
    public interface IMyObject
    {
        [OperationContract] // Делегируемый метод.
        string LoadData(string data, DateTime date,string type, int UserID);
        [OperationContract]
        int GetUserID(string UserName,string ComputerName);
        [OperationContract]
        bool check();
    }
}
