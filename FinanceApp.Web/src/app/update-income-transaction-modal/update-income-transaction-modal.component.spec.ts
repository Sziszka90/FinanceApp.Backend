import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateIncomeTransactionModalComponent } from './update-income-transaction-modal.component';

describe('IncomeTransactionModalComponent', () => {
  let component: UpdateIncomeTransactionModalComponent;
  let fixture: ComponentFixture<UpdateIncomeTransactionModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateIncomeTransactionModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(UpdateIncomeTransactionModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
