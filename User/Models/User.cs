using System;

namespace MODSI_SQLRestAPI.UserAuth.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Group { get; set; }
        public string Salt { get; set; }
        public string Tel { get; set; }
        public byte[] Photo { get; set; }

        public User()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public User(string name, string email, string password, string username, string role, string group, string salt, string tel, byte[] photo)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new BadRequestException($"Name cannot be null");
            }
            if (string.IsNullOrEmpty(email))
            {
                throw new BadRequestException($"Email cannot be null");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new BadRequestException($"Password cannot be null");
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new BadRequestException($"Username cannot be null or empty");
            }
            if (string.IsNullOrEmpty(role))
            {
                throw new BadRequestException($"Role cannot be null or empty");
            }
            if (string.IsNullOrEmpty(group))
            {
                throw new BadRequestException($"Group cannot be null or empty");
            }

            Name = name;
            Email = email;
            Password = password;
            Username = username;
            Role = role;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Group = group;
            Salt = salt;
            Tel = tel;
            Photo = photo;
        }

        public User(string name, string email, string password, string username, string role, string group)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new BadRequestException($"Name cannot be null");
            }
            if (string.IsNullOrEmpty(email))
            {
                throw new BadRequestException($"Email cannot be null");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new BadRequestException($"Password cannot be null");
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new BadRequestException($"Username cannot be null or empty");
            }
            if (string.IsNullOrEmpty(role))
            {
                throw new BadRequestException($"Role cannot be null or empty");
            }
            if (string.IsNullOrEmpty(group))
            {
                throw new BadRequestException($"Group cannot be null or empty");
            }


            Name = name;
            Email = email;
            Password = password;
            Username = username;
            Role = role;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Group = group;

        }

        // get methos
        public string GetName()
        {
            return Name;
        }

        public string GetEmail()
        {
            return Email;
        }

        public string GetUsername()
        {
            return Username;
        }

        public string GetRole()
        {
            return Role;
        }

        public DateTime GetCreatedAt()
        {
            return CreatedAt;
        }

        public bool GetIsActive()
        {
            return IsActive;
        }

        public string GetGroup()
        {
            return Group;
        }

        public string GetPassword()
        {
            return Password;
        }

        public string GetSalt()
        {
            return Salt;
        }

        public string GetTel()
        {
            return Tel;
        }

        public byte[] GetPhoto()
        {
            return Photo;
        }

        // set methods
        public void SetName(string name)
        {
            Name = name;
        }

        public void SetEmail(string email)
        {
            Email = email;
        }

        public void SetUsername(string username)
        {
            Username = username;
        }

        public void SetRole(string role)
        {
            Role = role;
        }

        public void SetCreatedAt(DateTime createdAt)
        {
            CreatedAt = createdAt;
        }

        public void SetIsActive(bool isActive)
        {
            IsActive = isActive;
        }

        public void SetGroup(string group)
        {
            Group = group;
        }

        public void SetPassword(string password)
        {
            Password = password;
        }

        public void SetSalt(string salt)
        {
            Salt = salt;
        }

        public void SetTel(string tel)
        {
            Tel = tel;
        }

        public void SetPhoto(byte[] photo)
        {
            Photo = photo;
        }
    }
}
