using Eli.ECS;

namespace Eli
{
	public class PaletteCyclerMaterial : Material<PaletteCyclerEffect>
	{
		public PaletteCyclerMaterial()
		{
			Effect = new PaletteCyclerEffect();
		}

		public override void OnPreRender(ICamera camera)
		{
			Effect.UpdateTime();
		}
	}
}