export const authConfig = {
    authority: 'http://localhost:5000',  
    client_id: 'react-client',          
    redirect_uri: 'http://localhost:3000/callback',  
    response_type: 'code',
    scope: 'openid profile api1',  
    post_logout_redirect_uri: 'http://localhost:3000',
  };
  