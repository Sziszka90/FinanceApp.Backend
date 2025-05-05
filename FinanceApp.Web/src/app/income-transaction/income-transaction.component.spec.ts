import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IncomeTransactionComponent } from './income-transaction.component';

describe('IncomeTransactionComponent', () => {
  let component: IncomeTransactionComponent;
  let fixture: ComponentFixture<IncomeTransactionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IncomeTransactionComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(IncomeTransactionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
