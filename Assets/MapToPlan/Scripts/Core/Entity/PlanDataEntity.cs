using System.Collections.Generic;

namespace MapToPlan.Scripts.Core.Entity
{
    public class PlanDataEntity
    {
        public PlanDataEntity()
        {
            Features = new List<PlanFeature>();
        }
        
        public List<PlanFeature> Features { get; set; }

        public void Clear()
        {
            foreach (var planFeature in Features)
            {
                planFeature.Clean();
                planFeature.CleanModifiers();
            }
            
            Features.Clear();
            // foreach (Transform children in Parent)
            // {
            //     Object.Destroy(children.gameObject);
            // }
        }
    }
}