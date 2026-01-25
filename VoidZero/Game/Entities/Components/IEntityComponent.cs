using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Game.Entities.Components
{
    public interface IEntityComponent
    {
        void Attach(Entity entity);
        void Update(Entity entity, float dt);
    }
}
