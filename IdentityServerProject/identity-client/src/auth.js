import { UserManager } from 'oidc-client';
import { authConfig } from './auth-config';

class AuthService {
  constructor() {
    this.userManager = new UserManager(authConfig);
  }

  login = () => {
    this.userManager.signinRedirect();
  };

  logout = () => {
    this.userManager.signoutRedirect();
  };

  getUser = async () => {
    return await this.userManager.getUser();
  };

  signinCallback = async () => {
    return await this.userManager.signinRedirectCallback();
  };
}

// eslint-disable-next-line import/no-anonymous-default-export
export default new AuthService();
