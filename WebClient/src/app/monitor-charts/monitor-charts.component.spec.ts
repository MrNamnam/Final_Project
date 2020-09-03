import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MonitorChartsComponent } from './monitor-charts.component';

describe('MonitorChartsComponent', () => {
  let component: MonitorChartsComponent;
  let fixture: ComponentFixture<MonitorChartsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MonitorChartsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MonitorChartsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
