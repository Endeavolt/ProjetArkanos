using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player
{
    public interface CharacterControlsInterface
    { 
        /// <summary>
        /// Give to player the controls of this mechanics
        /// </summary>
        /// <param name="stateControls"></param>
        public void ChangeControl(bool stateControls);

    }
}
