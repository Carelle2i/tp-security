import React, { useEffect, useState } from 'react';
import axios from 'axios';

const Articles = ({ user }) => {
  const [articles, setArticles] = useState([]);
  const [profile, setProfile] = useState(null);

  useEffect(() => {
    if (user) {
      // Récupérer le profil et les articles
      setProfile({
        name: user.profile.name,
        age: new Date().getFullYear() - new Date(user.profile.birthdate).getFullYear(),
        roles: user.profile.role,
      });

      // Récupérer les articles depuis l'API
      axios.get('http://localhost:5000/api/articles/public')
        .then(response => setArticles(response.data))
        .catch(error => console.error('Error fetching articles', error));
    }
  }, [user]);

  return (
    <div>
      <h1>Articles</h1>
      {profile && (
        <div>
          <p>Name: {profile.name}</p>
          <p>Age: {profile.age}</p>
          <p>Roles: {profile.roles}</p>
          {profile.roles.includes('Admin') && <button>Manage Articles</button>}
          {profile.roles.includes('Utilisateur') && <button>Your Articles</button>}
        </div>
      )}
      <ul>
        {articles.map((article) => (
          <li key={article.id}>{article.title}</li>
        ))}
      </ul>
    </div>
  );
};
export default Articles;
