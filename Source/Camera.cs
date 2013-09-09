using System;
using Microsoft.Xna.Framework;
using GameTimer;

namespace CameraBuddy
{
	public class Camera
	{
		#region Members

		//The previous coordinate system
		float m_fPrevLeft;
		float m_fPrevRight;
		float m_fPrevTop;
		float m_fPrevBottom;

		//the target coordinate system, will fit every object onto the screen.
		float m_fLeft;
		float m_fRight;
		float m_fTop;
		float m_fBottom;

		/// <summary>
		/// flag used to reset the camera each frame
		/// </summary>
		bool m_bCameraReset;

		/// <summary>
		/// this is whether to shake the camera left or right
		/// </summary>
		bool m_bShakeLeft;

		/// <summary>
		/// Clock used for camera movement and timing
		/// </summary>
		GameClock m_CameraClock;

		/// <summary>
		/// This is the time the camera started shaking
		/// </summary>
		GameClock m_ShakeTimer;

		//the amount of time to shake the camera
		private float m_fShakeTimeDelta;// = 0.25f;

		/// <summary>
		/// the amount to zoom during a camera shake
		/// this is a number between 0.0 and 1.0
		/// smaller = more shake
		/// </summary>
		private const float g_CAMERA_SHAKE_ZOOM = 0.02f;

		/// <summary>
		/// the amount to multiply the rotation of during the shake
		/// </summary>
		private const float g_CAMERA_SHAKE_ROTATE = 25.0f;

		/// <summary>
		/// how fast the camera can pan
		/// </summary>
		private const float g_CAMERA_SPEED = 4.5f;

		private const float g_ScaleSpeed = 16.0f;

		#endregion //Members

		#region Properties

		/// <summary>
		/// teh position to use as the camera origin
		/// </summary>
		private Vector2 Origin;

		/// <summary>
		/// The amount to scale the rendering
		/// </summary>
		public float Scale { get; set; }

		/// <summary>
		/// the previous value of Scale
		/// </summary>
		private float m_fPrevScale;

		/// <summary>
		/// The boundary we don't want the camera to leave!
		/// </summary>
		/// <value>The world.</value>
		public Rectangle WorldBoundary { get; set; }

		/// <summary>
		/// This is a flag that can be used if your game doesn't have a world boundary
		/// </summary>
		/// <value><c>true</c> if ignore world boundary; otherwise, <c>false</c>.</value>
		public bool IgnoreWorldBoundary { get; private set; }

		/// <summary>
		/// translation matrix for the camera
		/// </summary>
		public Matrix TranslationMatrix { get; private set; }

		/// <summary>
		/// the title safe area of the game window
		/// </summary>
		public Rectangle TitleSafeArea { private get; set; }

		/// <summary>
		/// the whole game window
		/// </summary>
		public Rectangle ScreenRect { private get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="CameraBuddy.Camera"/> class.
		/// </summary>
		public Camera()
		{
			Scale = 1.0f;
			m_fPrevScale = 1.0f;
			Origin = Vector2.Zero;

			m_fPrevLeft = 0.0f;
			m_fPrevRight = 0.0f;
			m_fPrevTop = 0.0f;
			m_fPrevBottom = 0.0f;
			m_fLeft = 0.0f;
			m_fRight = 0.0f;
			m_fTop = 0.0f;
			m_fBottom = 0.0f;
			m_bShakeLeft = true;
			m_bCameraReset = true;
			m_CameraClock = new GameClock();
			m_ShakeTimer = new GameClock();
			m_fShakeTimeDelta = 0.0f;
			WorldBoundary = new Rectangle(0, 0, 0, 0);
			IgnoreWorldBoundary = false;

			TranslationMatrix = Matrix.Identity;
			TitleSafeArea = new Rectangle();
			ScreenRect = new Rectangle();
		}

		/// <summary>
		/// Sets the screen rects.
		/// </summary>
		/// <param name="rScreen">R screen.</param>
		/// <param name="rTitleSafeArea">R title safe area.</param>
		public void SetScreenRects(Rectangle rScreen, Rectangle rTitleSafeArea)
		{
			TitleSafeArea = rTitleSafeArea;
			ScreenRect = rScreen;
		}

		/// <summary>
		/// Called to reset the camera view each frame
		/// Make sure to call this before any pionts are added to the scene
		/// </summary>
		public void Update(GameClock rGameTime)
		{
			m_bCameraReset = true;
			m_CameraClock.Update(rGameTime);
			m_ShakeTimer.Update(m_CameraClock);
		}

		/// <summary>
		/// Add a point to the scene.
		/// This gets called multiple times during the update loop to add all the points we want in frame.
		/// Later, it will calculate a scale and translation matrix to fit them all on screen.
		/// </summary>
		/// <param name="myPoint">the point that we want to be seen in the camera</param>
		public void AddPoint(Vector2 myPoint)
		{
			if (m_bCameraReset)
			{
				//first point this frame, reset the viewport
				m_fLeft = myPoint.X;
				m_fRight = myPoint.X;
				m_fBottom = myPoint.Y;
				m_fTop = myPoint.Y;

				m_bCameraReset = false;
			}
			else
			{
				//check the left & right
				if (myPoint.X < m_fLeft)
				{
					m_fLeft = myPoint.X;
				}
				else if (myPoint.X > m_fRight)
				{
					m_fRight = myPoint.X;
				}

				//check the top & bottom
				if (myPoint.Y < m_fTop)
				{
					m_fTop = myPoint.Y;
				}
				else if (myPoint.Y > m_fBottom)
				{
					m_fBottom = myPoint.Y;
				}
			}
		}

		/// <summary>
		/// Call this before spritebatch.begin is called to set the matrixes up
		/// After this is called, get the TranslationMatrix and pass it into spritebatch.begin
		/// </summary>
		/// <param name="bForceToViewport">Can pass "true" to force it to snap to the required matrix, pass "false" to slowly transition</param>
		public void BeginScene(bool bForceToViewport)
		{
			MoveToViewport(bForceToViewport);

			//setup the scale matrix
			Matrix ScaleMatrix = Matrix.CreateScale(Scale);

			//setup the translation matrix
			Vector2 translationVect = new Vector2(
				(TitleSafeArea.Width / 2.0f) - (Scale * Origin.X),
				(TitleSafeArea.Height / 2.0f) - (Scale * Origin.Y));
			TranslationMatrix = Matrix.CreateTranslation(translationVect.X, translationVect.Y, 0.0f);

			TranslationMatrix = Matrix.Multiply(ScaleMatrix, TranslationMatrix);
		}

		/// <summary>
		/// Moves to viewport.
		/// Can pass "true" to force it to snap to the required matrix, pass "false" to slowly transition
		/// </summary>
		/// <param name="bForceToViewport">If set to <c>true</c> b force to viewport.</param>
		private void MoveToViewport(bool bForceToViewport)
		{
			//set the camera position 
			if (bForceToViewport)
			{
				m_fPrevLeft = m_fLeft;
				m_fPrevRight = m_fRight;
				m_fPrevTop = m_fTop;
				m_fPrevBottom = m_fBottom;
			}
			else
			{
				//move the left
				float fLeftDiff = (((m_fLeft - m_fPrevLeft) * g_CAMERA_SPEED) * m_CameraClock.TimeDelta);
				m_fPrevLeft += fLeftDiff;
				m_fLeft = m_fPrevLeft;

				//move the Right
				float fRightDiff = (((m_fRight - m_fPrevRight) * g_CAMERA_SPEED) * m_CameraClock.TimeDelta);
				m_fPrevRight += fRightDiff;
				m_fRight = m_fPrevRight;

				//move the Top
				float fTopDiff = (((m_fTop - m_fPrevTop) * g_CAMERA_SPEED) * m_CameraClock.TimeDelta);
				m_fPrevTop += fTopDiff;
				m_fTop = m_fPrevTop;

				//move the Bottom
				float fBottomDiff = (((m_fBottom - m_fPrevBottom) * g_CAMERA_SPEED) * m_CameraClock.TimeDelta);
				m_fPrevBottom += fBottomDiff;
				m_fBottom = m_fPrevBottom;
			}

			//add the camera spin
			if (m_ShakeTimer.CurrentTime < m_fShakeTimeDelta)
			{
				//figure out the proper rotation for the camera shake
				float fShakeX = (g_CAMERA_SHAKE_ROTATE *
					(float)Math.Sin(
					((m_ShakeTimer.CurrentTime * (2.0f * Math.PI)) /
					m_fShakeTimeDelta)));

				float fShakeY = (g_CAMERA_SHAKE_ROTATE *
					(float)Math.Cos(
					((m_ShakeTimer.CurrentTime * (2.0f * Math.PI)) /
					m_fShakeTimeDelta)));

				fShakeX = fShakeX * (m_bShakeLeft ? -1.0f : 1.0f);

				m_fLeft += fShakeX;
				m_fRight += fShakeX;

				m_fTop += fShakeY;
				m_fBottom += fShakeY;
			}

			//Constrain the camera to stay in teh game world... or dont.
			if (!IgnoreWorldBoundary)
			{
				//hold all points inside the game world!
				if (m_fTop < WorldBoundary.Top)
				{
					m_fTop = WorldBoundary.Top;
				}
			
				if (m_fBottom > WorldBoundary.Bottom)
				{
					m_fBottom = WorldBoundary.Bottom;
				}

				if (m_fLeft < WorldBoundary.Left)
				{
					m_fLeft = WorldBoundary.Left;
				}

				if (m_fRight > WorldBoundary.Right)
				{
					m_fRight = WorldBoundary.Right;
				}
			}
			else
			{
				//Let the camera roam around wherever it wants
				WorldBoundary = new Rectangle(
					(int)m_fLeft, 
					(int)m_fTop, 
					(int)(m_fRight - m_fLeft), 
					(int)(m_fBottom - m_fTop));
			}

			//check if we need to zoom to fit either horizontal or vertical

			//get the current aspect ratio
			float fScreenAspectRatio = (float)ScreenRect.Width / (float)ScreenRect.Height;

			//get the current target aspect ratio
			float fWidth = m_fRight - m_fLeft;
			float fHeight = m_fBottom - m_fTop;
			float fMyAspectRatio = fWidth / fHeight;

			/*
			Here's the formula:

			X   A+i
			- = ---
			Y   B+j

			where x/y is the screen aspect ratio and A+i/B+j is the current target.
			*/

			//If the current target is less than gl aspect ratio, B is too big (too tall)
			float fNewScale = 1.0f;
			if (fMyAspectRatio < fScreenAspectRatio)
			{
				//j = 0.0f, figure for i
				//increase the A (width) to fit the aspect ratio
				float fTotalAdjustment = (fScreenAspectRatio * fHeight) - fWidth;

				//don't let it go past the walls
				float fAdjustedRight = (m_fRight + (fTotalAdjustment / 2.0f));
				float fAdjustedLeft = (m_fLeft - (fTotalAdjustment / 2.0f));

				bool bOffRightWall = fAdjustedRight > WorldBoundary.Right;
				bool bOffLeftWall = fAdjustedLeft < WorldBoundary.Left;

				if (!bOffRightWall && !bOffLeftWall)
				{
					//those new limits are fine!
					m_fRight = fAdjustedRight;
					m_fLeft = fAdjustedLeft;
				}
				else if (bOffRightWall && bOffLeftWall)
				{
					//both limits are screwed up
					m_fRight = WorldBoundary.Right;
					m_fLeft = WorldBoundary.Left;
				}
				else
				{
					//have to adjust to keep the camera on the board

					if (bOffRightWall && !bOffLeftWall)
					{
						//put the right at the wall
						float fRightAdjustment = fAdjustedRight - WorldBoundary.Right;
						m_fRight = WorldBoundary.Right;
						m_fLeft -= ((fTotalAdjustment / 2.0f) + fRightAdjustment);
					}
					else if (!bOffRightWall && bOffLeftWall)
					{
						//put the left at the wall
						float fLeftAdjustment = WorldBoundary.Left - fAdjustedLeft;
						m_fLeft = WorldBoundary.Left;
						m_fRight += (fTotalAdjustment / 2.0f) + fLeftAdjustment;
					}

					//ok double check those limits are still good after being adjusted
					if (m_fRight > WorldBoundary.Right)
					{
						m_fRight = WorldBoundary.Right;
					}
					if (m_fLeft < WorldBoundary.Left)
					{
						m_fLeft = WorldBoundary.Left;
					}
				}

				fNewScale = ScreenRect.Width / (m_fRight - m_fLeft);
			}
			//If the current target is greater than gl aspect ratio, A is too big (too wide)
			else if (fMyAspectRatio > fScreenAspectRatio)
			{
				//i = 0.0f, figure for j
				//increase the B (height) to fit the aspect ratio
				float fTotalAdjustment = ((fWidth * (float)ScreenRect.Height) / (float)ScreenRect.Width) - fHeight;

				//if that moves below the bottom, add it all to the top
				float fAdjustedBottom = (m_fBottom + (fTotalAdjustment / 2.0f)); //this is where the bottom will be
				float fAdjustedCeiling = (m_fTop - (fTotalAdjustment / 2.0f)); //this is where ceiling will be

				bool bOffBottomWall = fAdjustedBottom > WorldBoundary.Bottom;
				bool bOffCeilingWall = fAdjustedCeiling < WorldBoundary.Top;

				if (!bOffBottomWall && !bOffCeilingWall)
				{
					//those new limits are fine!
					m_fTop = fAdjustedCeiling;
					m_fBottom = fAdjustedBottom;
				}
				else if (bOffBottomWall && bOffCeilingWall)
				{
					//both limits are screwed up
					m_fBottom = WorldBoundary.Bottom;
					m_fTop = WorldBoundary.Top;
				}
				else
				{
					//have to adjust to keep the camera on the board

					if (bOffBottomWall && !bOffCeilingWall)
					{
						//put the bottom at the floor
						float fBottomAdjustment = fAdjustedBottom - WorldBoundary.Bottom;
						m_fBottom = WorldBoundary.Bottom;
						m_fTop -= ((fTotalAdjustment / 2.0f) + fBottomAdjustment);
					}
					else if (!bOffBottomWall && bOffCeilingWall)
					{
						//put the top at the ceiling
						float fTopAdjustment = WorldBoundary.Top - fAdjustedCeiling;
						m_fTop = WorldBoundary.Top;
						m_fBottom += (fTotalAdjustment / 2.0f) + fTopAdjustment;
					}

					//ok double check those limits are still good after being adjusted
					if (m_fBottom > WorldBoundary.Bottom)
					{
						m_fBottom = WorldBoundary.Bottom;
					}
					if (m_fTop < WorldBoundary.Top)
					{
						m_fTop = WorldBoundary.Top;
					}
				}

				fNewScale = ScreenRect.Height / (m_fBottom - m_fTop);
			}

			//set the camera scale
			if (bForceToViewport)
			{
				Scale = fNewScale;
			}
			else
			{
				Scale += (((fNewScale - m_fPrevScale) * g_ScaleSpeed) * m_CameraClock.TimeDelta);
			}
			m_fPrevScale = Scale;

			//set teh camer position to be the center of the desired rectangle;
			Origin.X = ((m_fLeft + m_fRight) / 2.0f) - (TitleSafeArea.Left / Scale);
			Origin.Y = ((m_fTop + m_fBottom) / 2.0f) - (TitleSafeArea.Top / Scale);

			if (m_ShakeTimer.CurrentTime < m_fShakeTimeDelta)
			{
				/*
				okay the formula for camera shake rotation:

				sin(

				current		=		x 
				-------			--------
				shakedellta			2pi
				
				)

				multiply by a number 0 > 1 to increase amplitude of the camera rotate
				*/

				//figure how much camera shake to add to the zoom
				float fShakeZoom = ((Scale * g_CAMERA_SHAKE_ZOOM) *
					(float)Math.Sin(
					((m_ShakeTimer.CurrentTime * Math.PI) /
					m_fShakeTimeDelta)));
				Scale *= 1.0f + fShakeZoom;
			}
		}

		/// <summary>
		/// add some camera shaking!
		/// </summary>
		/// <param name="fTimeDelta">how long to shake the camera</param>
		public void AddCameraShake(float fTimeDelta)
		{
			m_ShakeTimer.Start();
			m_bShakeLeft = !m_bShakeLeft;
			m_fShakeTimeDelta = fTimeDelta;
		}

		/// <summary>
		/// Forces to screen.  Same as doing BeginScene(true)
		/// </summary>
		public void ForceToScreen()
		{
			BeginScene(true);
		}

//		public void DrawCameraInfo(IRenderer rRenderer)
//		{
//			//draw the center point
//			rRenderer.DrawPoint(m_Origin, Color.Red);
//		}

		#endregion //Methods
	}
}