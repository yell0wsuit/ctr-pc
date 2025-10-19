﻿using System;
using CutTheRope.iframework;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x02000075 RID: 117
	internal class CTRGameObject : GameObject
	{
		// Token: 0x06000481 RID: 1153 RVA: 0x0001A0A8 File Offset: 0x000182A8
		public override void parseMover(XMLNode xml)
		{
			this.rotation = 0f;
			NSString nSString = xml["angle"];
			if (nSString != null)
			{
				this.rotation = nSString.floatValue();
			}
			NSString nSString2 = xml["path"];
			if (nSString2 != null && nSString2.length() != 0)
			{
				int i = 100;
				if (nSString2.characterAtIndex(0) == 'R')
				{
					i = (int)((float)((int)FrameworkTypes.RTD((double)nSString2.substringFromIndex(2).intValue())) * 3.3f) / 2 + 1;
				}
				float m_ = xml["moveSpeed"].floatValue() * 3.3f;
				float r_ = xml["rotateSpeed"].floatValue();
				CTRMover cTRMover = (CTRMover)new CTRMover().initWithPathCapacityMoveSpeedRotateSpeed(i, m_, r_);
				cTRMover.angle_ = (double)this.rotation;
				cTRMover.angle_initial = cTRMover.angle_;
				cTRMover.setPathFromStringandStart(nSString2, MathHelper.vect(this.x, this.y));
				this.setMover(cTRMover);
				cTRMover.start();
			}
		}
	}
}
