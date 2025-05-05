import { Routes } from '@angular/router';
import { IncomeTransactionComponent } from './income-transaction/income-transaction.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegistrationComponent } from './registration/registration.component';
import { LoggedInComponent } from './logged-in/logged-in.component';
import { ProfileComponent } from './profile/profile.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'logged-in', component: LoggedInComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'registration', component: RegistrationComponent },
  { path: 'incometransactions', component: IncomeTransactionComponent },
  { path: '**', component: NotFoundComponent }, // Redirect to Home for invalid paths
];
