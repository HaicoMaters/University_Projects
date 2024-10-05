using System.Collections;
using System.Collections.Generic;
namespace AI
{
    public class TreeBasedGameConfiguration<T>
    {
        public List<T> actionsPerformed;

        public TreeBasedGameConfiguration()
        {
            actionsPerformed = new List<T>();
        }

        public TreeBasedGameConfiguration<T> appendAction(T action)
        {
            var conf = new TreeBasedGameConfiguration<T>();
            foreach (var x in actionsPerformed) conf.actionsPerformed.Add(x);
            conf.actionsPerformed.Add(action);
            return conf;
        }
    }
}