using Pixelplacement;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class PointerLerp : MonoBehaviour
	{
		#region vars
		#region public vars
		
		#endregion public vars
		#region private vars
		
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			this.WaitS(3, Activate);	
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers

		#endregion event handlers

		#region helper methods
		public void Activate()
		{
			ArrowToggler.Instance.Toggle(0, true);
			this.LerpRight(0.2f, 0.5f, () => {
				this.LerpRight(-0.4f, 1, ()=> {
					this.LerpRight(0.2f, 0.5f, ()=>{
						ArrowToggler.Instance.Toggle(0, false);
						ArrowToggler.Instance.Toggle(1, true);
						this.LerpDown(-0.2f, 0.5f, () => {
							this.LerpDown(0.4f, 1f, () => {
								this.LerpDown(-0.2f, 0.5f, () => {
									ArrowToggler.Instance.Toggle(1, false);
									this.WaitS(1, () => {
										ArrowToggler.Instance.Toggle(2, true);
										this.LerpForward(-0.2f, 0.5f, () => {
											this.LerpForward(0.4f, 1f, () => {
												this.LerpForward(-0.2f, 0.5f, () => {
													this.WaitS(0.3f,()=> ArrowToggler.Instance.Toggle(2, false));
												});
											});
										});
									});
								});
							});
						});
					});
				});
			});
		}
		#endregion helper methods

		#region Unit Tests

		#endregion Unit Tests
	}
}
