using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EnemyDATA
{
    public class EnemyManager
    {
        public Container container;
        public EnemyManager(GameObject me)
        {
            container = new Container(this, me);
        }
    }

    public class Container
    {
        public EnemyManager link;
        
        public Container(EnemyManager link, GameObject me)
        {
            this.link = link;
            
        }
    }

    public class SlimeDATA
    {
        public List<Slime> allSlime;

        public SlimeDATA()
        {
            allSlime = new List<Slime>();
        }

        private static SlimeDATA instance;

        public static SlimeDATA GetInstanse()
        {
            if (instance == null)
            {
                instance = new SlimeDATA();
            }

            return instance;
        }
    }
    
    

}

