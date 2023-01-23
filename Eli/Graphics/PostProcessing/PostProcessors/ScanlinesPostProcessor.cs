﻿using Eli.ECS;

namespace Eli
{
	public class ScanlinesPostProcessor : PostProcessor<ScanlinesEffect>
	{
		public ScanlinesPostProcessor(int executionOrder) : base(executionOrder)
		{
		}

		public override void OnAddedToScene(IScene scene)
		{
			base.OnAddedToScene(scene);
			Effect = _scene.Content.LoadNezEffect<ScanlinesEffect>();
		}

		public override void Unload()
		{
			_scene.Content.UnloadEffect(Effect);
			base.Unload();
		}
	}
}