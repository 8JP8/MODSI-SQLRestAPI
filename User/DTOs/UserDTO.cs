using System;

namespace MODSI_SQLRestAPI.UserAuth.DTO
{

    // DTO (Data Transfer Object) for User
    // This class is used to transfer user data without exposing sensitive information
    // Usar sempre que um user pede dados e como não queremos expor a dados sensiveis (password,salt,etc), usamos o DTO
    // O DTO pode ser usado tanto no backend como no frontend, mas como vamos usar Appsmith vamos então usar o DTO aqui para facilitar 
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Group { get; set; }
        public byte[] Photo { get; set; }


        public UserDTO()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
        public UserDTO(string name, string email, string username, string role, string group, byte[] photo)
        {
            Name = name;
            Email = email;
            Username = username;
            Role = role;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Group = group;
            Photo = photo;
        }
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
        public byte[] GetPhoto()
        {
            return Photo;
        }
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
        public void SetPhoto(byte[] photo)
        {
            Photo = photo;
        }

    }
}