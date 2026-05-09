using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Game.Entities.Components
{
    // Interface for the ECS we created by ourselves
    // We are able to attack components to Entities to give them more functionality
    public interface IEntityComponent
    {
        void Attach(Entity entity);
        void Update(Entity entity, float dt);
    }
}
