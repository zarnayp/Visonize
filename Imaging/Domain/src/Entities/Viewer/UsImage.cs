using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupploPulse.UsImaging.Domain.Entities.Viewer
{
    public class UsImage
    {
        public String Id { get; private set; }


        public UsImage(String id)
        {
            Id = id;
        }
    }
}
