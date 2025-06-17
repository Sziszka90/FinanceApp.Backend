import { Routes, CanActivateFn } from '@angular/router';
import { TransactionComponent } from './transaction/transaction.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegistrationComponent } from './registration/registration.component';
import { LoggedInComponent } from './logged-in/logged-in.component';
import { ProfileComponent } from './profile/profile.component';
import { TransactionGroupComponent } from './transaction-group/transaction-group.component';
import { TOKEN_KEY } from 'src/models/Constants/token.const';

// Simple AuthGuard implementation
const AuthGuard: CanActivateFn = () => {
  const token = localStorage.getItem(TOKEN_KEY); // or your auth logic
  return !!token;
};

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'logged-in', component: LoggedInComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'registration', component: RegistrationComponent },
  { path: 'transactions', component: TransactionComponent, canActivate: [AuthGuard] },
  { path: 'transactions-groups', component: TransactionGroupComponent, canActivate: [AuthGuard] },
  { path: '**', component: NotFoundComponent },
];
