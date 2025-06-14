using System;

namespace MODSI_SQLRestAPI.UserAuth.DTO
{

    // DTO (Data Transfer Object) for User
    // This class is used to transfer user data without exposing sensitive information
    // Usar sempre que um user pede dados e como n�o queremos expor a dados sensiveis (password,salt,etc), usamos o DTO
    // O DTO pode ser usado tanto no backend como no frontend, mas como vamos usar Appsmith vamos ent�o usar o DTO aqui para facilitar 
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string Group { get; set; }
        public string Photo { get; set; }
        public string Tel { get; set; }


        public UserDTO()
        {
            CreatedAt = DateTime.UtcNow;
            IsVerified = false;
        }
        public UserDTO(string name, string email, string username, string role, string group, string photo, string tel)
        {
            Name = name;
            Email = email;
            Username = username;
            Role = role ?? "n.d.";
            CreatedAt = DateTime.UtcNow;
            IsVerified = false;
            Group = group ?? "n.d.";
            Photo = photo;
            Tel = tel;
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
        public bool GetIsVerified()
        {
            return IsVerified;
        }
        public string GetGroup()
        {
            return Group;
        }
        public string GetPhoto()
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
        public void SetIsVerified(bool isVerified)
        {
            IsVerified = isVerified;
        }
        public void SetGroup(string group)
        {
            Group = group;
        }
        public void SetPhoto(string photo)
        {
            Photo = photo;
        }

    }
}