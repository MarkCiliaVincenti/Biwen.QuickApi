��Լ򵥵Ķ���ӳ��,�Ƽ�ʹ��`Biwen.AutoClassGen`����,�ṩ�������,
[�����ĵ�](https://github.com/vipwan/Biwen.AutoClassGen/blob/master/Gen-Dto.md)

��Ҳ����ʹ������`Maspter`,������ע��`IMapper`:
```csharp
    //����Mapping
    public class MappingTest
    {
        [Fact]
        public void IMapperTest()
        {
            var services = new ServiceCollection();
            services.AddMapsterMapper();
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<IMapper>();

            var user = new User
            {
                Name = "Tom",
                Age = 10,
                Address = "China"
            };

            var userDto = mapper.Map<UserDto>(user);
            Assert.NotNull(userDto);
            Assert.Equal(user.Name, userDto.Name);
            Assert.Equal(user.Age, userDto.Age);
        }

        #region model

        //����User����:
        public class User
        {
            public string? Name { get; set; }
            public int Age { get; set; }

            public string? Address { get; set; }
        }

        //����UserDto����:
        public class UserDto
        {
            public string? Name { get; set; }
            public int Age { get; set; }
        }
        #endregion
    }
```