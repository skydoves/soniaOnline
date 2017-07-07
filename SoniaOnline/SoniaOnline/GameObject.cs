using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SoniaOnline
{
    class GameObject
    {
        // 객체의 이름
        public string name;

        // 이미지 스프라이트
        public Texture2D sprite;

        // 위치값
        public Vector2 position;
        public Vector2 center;

        // 방향, 속도 값
        public float rotation;
        public Vector2 velocity;

        // 생명도
        public bool alive;

        // # 객체값의 오버라이딩 #
        public GameObject(Texture2D TextureImage)
        {
            // 이름의 초기화
            name = "";

            // 이미지의 초기화
            sprite = TextureImage;

            // 위치값 초기화
            position = new Vector2(-sprite.Width, -sprite.Height);
            center = new Vector2(sprite.Width / 2, sprite.Height / 2);

            // 방향, 속도 값
            rotation = 0.0f;
            velocity = new Vector2(0, 0);

            // 생명도
            alive = true;
        }

        // # 객체 이름 설정 #
        public void SetName(string name)
        {
            this.name = name;
        }
    }
}
