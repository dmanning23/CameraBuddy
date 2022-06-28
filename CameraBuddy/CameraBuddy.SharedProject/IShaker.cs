using GameTimer;

namespace CameraBuddy
{
    public interface IShaker
    {
        float ShakeAmount { get; }

        bool ShakeLeft { get; }

        CountdownTimer ShakeTimer { get; }

        void AddShake(float length, float delta, float amount);

        void SetShake(float length, float delta, float amount);

        void StopShake();
    }
}
