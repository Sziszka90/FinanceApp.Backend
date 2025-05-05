import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateIncomeTransactionModalComponent } from './create-income-transaction-modal.component';

describe('IncomeTransactionModalComponent', () => {
  let component: CreateIncomeTransactionModalComponent;
  let fixture: ComponentFixture<CreateIncomeTransactionModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateIncomeTransactionModalComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(CreateIncomeTransactionModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
