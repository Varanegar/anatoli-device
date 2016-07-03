using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Manager
{
    public class MessageManager : BaseManager<MessageModel>
    {

        public static bool Delete(int id)
        {
            DeleteCommand command = new DeleteCommand("messages", new EqFilterParam("msg_id", id.ToString()));
            var result = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
            return (result > 0) ? true : false;
        }

        public static void SetViewFlag(List<int> ids)
        {
            try
            {
                string q = "UPDATE Message SET NewFlag=1 WHERE ";
                foreach (var item in ids)
                {
                    q += " UniqueId=" + item.ToString() + " OR";
                }
                q += " 1=0";

                StringQuery command = new StringQuery(q);
                command.Unlimited = true;
                var result = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
            }
            catch (Exception)
            {
                return;
            }
        }
        public override int UpdateItem(MessageModel model)
        {
            throw new NotImplementedException();
        }
        public static StringQuery GetAllQueryString()
        {
            return new StringQuery("SELECT * FROM Message");
        }
    }
}
